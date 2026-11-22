using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Sequencer.Audio.Effects;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Audio
{
    /// <summary>
    /// Plays the song: on each clock tick, works out which section is playing and starts and stops each
    /// track's notes and drum hits with that track's instrument. Every sound is a voice in one mixer, which
    /// plays through one output stream.
    /// </summary>
    [UsedImplicitly]
    public sealed class SequencerPlayer : IDisposable
    {
        private const double PreviewSeconds = 0.15;

        [NotNull] private readonly ISequencerClock clock;
        [NotNull] private readonly MixingSampleProvider mixer;
        [NotNull] private readonly LimiterSampleProvider limiter;
        [NotNull] private readonly IAudioOutput output;
        [NotNull] private readonly ISignalProviderFactory signalProviderFactory;
        [NotNull] private readonly ITickCalculator tickCalculator;
        [NotNull] private readonly Song song;
        [NotNull] private readonly ArrangementState state;
        private readonly int sampleRate;
        private readonly int previewSamples;

        // Ticks arrive on the clock thread; Stopped and mute changes can come from the UI thread.
        [NotNull] private readonly object voicesLock = new object();
        [NotNull] private readonly Dictionary<ISequencedNote, SoundingNote> voices = new Dictionary<ISequencedNote, SoundingNote>();

        // Each track's channel, where its voices are mixed and its effects applied.
        [NotNull] private readonly Dictionary<Track, TrackBus> buses = new Dictionary<Track, TrackBus>();

        // Each drum track's open hi-hat still ringing, so its next hat can cut it off.
        [NotNull] private readonly Dictionary<Track, DrumVoice> ringingOpenHats = new Dictionary<Track, DrumVoice>();
        private volatile bool isRunning;

        public SequencerPlayer([NotNull] ISignalProviderFactory signalProviderFactory, [NotNull] Song song, [NotNull] ArrangementState state,
            [NotNull] ISequencerClock clock, [NotNull] ITickCalculator tickCalculator, [NotNull] MixerSettings mixerSettings,
            [NotNull] IAudioOutput output)
        {
            this.signalProviderFactory = signalProviderFactory;
            this.song = song;
            this.state = state;
            this.clock = clock;
            this.tickCalculator = tickCalculator;
            this.output = output;
            sampleRate = mixerSettings.SampleRate;
            previewSamples = (int) (PreviewSeconds * sampleRate);

            // ReadFully keeps the stream running with silence when nothing is playing.
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1)) { ReadFully = true };

            // Master volume, then a limiter so overlapping notes can never clip.
            limiter = new LimiterSampleProvider(new MasterVolumeSampleProvider(mixer, mixerSettings));
            output.Play(limiter);

            song.Changed += (sender, args) => RetireRemovedTracks();

            clock.Tick += OnTick;
            clock.Started += OnClockStarted;
            clock.Stopped += OnClockStopped;
        }

        /// <summary>
        /// The number of sounds playing, including notes in their release stage and drum hits.
        /// </summary>
        public int ActiveVoiceCount
        {
            get
            {
                lock (buses)
                {
                    return buses.Values.Sum(bus => bus.VoiceCount);
                }
            }
        }

        /// <summary>
        /// The fraction of the time the master limiter has had to turn the level down.
        /// </summary>
        public double LimiterActivity => limiter.Activity;

        /// <summary>
        /// The loudest the mix has been before the limiter.
        /// </summary>
        public float PeakBeforeLimiter => limiter.PeakInput;

        /// <summary>
        /// Ticks in one four-bar section.
        /// </summary>
        public int TicksPerSection => Math.Max(1, clock.TicksPerQuarterNote * 16);

        private int TicksPerSixteenth => Math.Max(1, clock.TicksPerQuarterNote / 4);

        /// <summary>
        /// Plays a short, self-releasing note on a track's instrument. Safe to call from the UI thread.
        /// </summary>
        public void Preview([NotNull] Tone tone, [NotNull] Track track)
        {
            ISignalProvider voice = CreateVoice(tone, track);
            voice.NoteOn();
            voice.ReleaseAfter(previewSamples);
            BusFor(track).AddVoice(voice);
        }

        /// <summary>
        /// Starts a note on a track's instrument that plays until the returned handle is disposed.
        /// </summary>
        [NotNull]
        public IDisposable Hold([NotNull] Tone tone, [NotNull] Track track)
        {
            ISignalProvider voice = CreateVoice(tone, track);
            voice.NoteOn();
            BusFor(track).AddVoice(voice);
            return new HeldNote(voice);
        }

        /// <summary>
        /// Plays one drum hit on a drum track's kit now. Safe to call from the UI thread.
        /// </summary>
        public void PreviewDrum(DrumSound sound, byte velocity, [NotNull] Track track)
        {
            PlayDrum(sound, velocity, track);
        }

        public void Dispose()
        {
            clock.Tick -= OnTick;
            clock.Started -= OnClockStarted;
            clock.Stopped -= OnClockStopped;
            output.Dispose();
        }

        private void OnClockStarted(object sender, EventArgs e)
        {
            isRunning = true;
        }

        private void OnClockStopped(object sender, EventArgs e)
        {
            isRunning = false;
            ReleaseNotes(voice => true);
        }

        private void OnTick(object sender, [NotNull] TickEventArgs e)
        {
            // Stop and Pause also raise a tick to update the position display. Those must not start notes.
            if (!isRunning)
            {
                return;
            }

            int section = state.Mode == PlaybackMode.Song ? e.CurrentTick / TicksPerSection : state.LoopSection;
            int tickInSection = e.CurrentTick % TicksPerSection;

            if (tickInSection == 0)
            {
                // A new section (or the loop wrapped). Notes never cross a section, and one ending exactly at its
                // end never sees its ending tick (that tick is the next section's 0), so release everything.
                ReleaseNotes(voice => true);
            }

            IPosition position = tickCalculator.CalculatePositionFromTick(tickInSection);
            bool onSixteenth = tickInSection % TicksPerSixteenth == 0;

            foreach (Track track in song.Tracks)
            {
                if (track.IsMuted)
                {
                    ReleaseNotes(voice => voice.Track == track);
                    continue;
                }

                switch (track.ClipAt(section))
                {
                    case NoteClip notes:
                        StartNotes(notes, position, track);
                        ReleaseEndingNotes(notes, position);
                        break;
                    case DrumClip drums when onSixteenth:
                        TriggerDrums(drums.Pattern, tickInSection / TicksPerSixteenth % DrumPattern.StepCount, track);
                        break;
                }
            }
        }

        private void StartNotes([NotNull] ISequencedNotes notes, [NotNull] IPosition position, [NotNull] Track track)
        {
            foreach (ISequencedNote note in notes.FindNotesFromStartingPosition(position))
            {
                lock (voicesLock)
                {
                    if (voices.ContainsKey(note))
                    {
                        continue;
                    }

                    ISignalProvider voice = CreateVoice(note.Tone, track);
                    voice.NoteOn();
                    voices.Add(note, new SoundingNote(voice, track));
                    BusFor(track).AddVoice(voice);
                }
            }
        }

        private void ReleaseEndingNotes([NotNull] ISequencedNotes notes, [NotNull] IPosition position)
        {
            foreach (ISequencedNote note in notes.FindNotesFromEndingPosition(position))
            {
                lock (voicesLock)
                {
                    if (voices.TryGetValue(note, out SoundingNote sounding))
                    {
                        // The voice stays in the mixer through its release stage and removes itself once silent.
                        sounding.Voice.NoteOff();
                        voices.Remove(note);
                    }
                }
            }
        }

        private void ReleaseNotes([NotNull] Func<SoundingNote, bool> which)
        {
            lock (voicesLock)
            {
                foreach (KeyValuePair<ISequencedNote, SoundingNote> sounding in voices.Where(pair => which(pair.Value)).ToList())
                {
                    sounding.Value.Voice.NoteOff();
                    voices.Remove(sounding.Key);
                }
            }
        }

        private void TriggerDrums([NotNull] DrumPattern pattern, int step, [NotNull] Track track)
        {
            foreach (DrumSound sound in DrumPattern.AllSounds)
            {
                byte velocity = pattern.VelocityAt(sound, step);

                if (velocity > 0)
                {
                    PlayDrum(sound, velocity, track);
                }
            }
        }

        private void PlayDrum(DrumSound sound, byte velocity, [NotNull] Track track)
        {
            var voice = new DrumVoice(sound, velocity, sampleRate, (float) (track.Drums?.Volume ?? 1) * MixLevels.DrumHit);

            // On a drum machine the hi-hats share one voice: a new hat cuts off an open one still ringing.
            if (sound == DrumSound.ClosedHat || sound == DrumSound.OpenHat)
            {
                lock (ringingOpenHats)
                {
                    if (ringingOpenHats.TryGetValue(track, out DrumVoice ringing))
                    {
                        ringing.Choke();
                        ringingOpenHats.Remove(track);
                    }

                    if (sound == DrumSound.OpenHat)
                    {
                        ringingOpenHats[track] = voice;
                    }
                }
            }

            BusFor(track).AddVoice(voice);
        }

        /// <summary>
        /// The track's channel, created the first time it plays.
        /// </summary>
        [NotNull]
        private TrackBus BusFor([NotNull] Track track)
        {
            lock (buses)
            {
                if (!buses.TryGetValue(track, out TrackBus bus))
                {
                    bus = new TrackBus(track, sampleRate, () => clock.BeatsPerMinute);
                    buses.Add(track, bus);
                    mixer.AddMixerInput(bus);
                }

                return bus;
            }
        }

        /// <summary>
        /// Fades out the channels of tracks no longer in the song (after a new song is opened, say).
        /// </summary>
        private void RetireRemovedTracks()
        {
            lock (buses)
            {
                foreach (Track gone in buses.Keys.Where(track => !song.Tracks.Contains(track)).ToList())
                {
                    buses[gone].Retire();
                    buses.Remove(gone);
                }
            }
        }

        [NotNull]
        private ISignalProvider CreateVoice([NotNull] Tone tone, [NotNull] Track track)
        {
            IAudioSettings instrument = track.Synth ?? throw new ArgumentException($"{track.Name} has no synth.", nameof(track));
            return signalProviderFactory.CreateSignalProvider(new PreviewNote(tone), instrument);
        }

        private readonly struct SoundingNote
        {
            public SoundingNote([NotNull] ISignalProvider voice, [NotNull] Track track)
            {
                Voice = voice;
                Track = track;
            }

            [NotNull]
            public ISignalProvider Voice { get; }

            [NotNull]
            public Track Track { get; }
        }

        private sealed class HeldNote : IDisposable
        {
            [NotNull] private readonly ISignalProvider voice;

            public HeldNote([NotNull] ISignalProvider voice)
            {
                this.voice = voice;
            }

            public void Dispose()
            {
                voice.NoteOff();
            }
        }

        private sealed class PreviewNote : ISequencedNote
        {
            public PreviewNote([NotNull] Tone tone)
            {
                Tone = tone;
            }

            public Tone Tone { get; }
        }
    }
}
