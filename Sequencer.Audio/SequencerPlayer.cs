using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Sequencer.Domain;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Audio
{
    /// <summary>
    /// Turns clock ticks into note-on and note-off events. Every sounding note is a voice in a single
    /// mixer, which plays through one output stream.
    /// </summary>
    [UsedImplicitly]
    public sealed class SequencerPlayer : IDisposable
    {
        private const double PreviewSeconds = 0.15;

        [NotNull] private readonly ISequencerClock clock;
        [NotNull] private readonly MixingSampleProvider mixer;
        [NotNull] private readonly IAudioOutput output;
        [NotNull] private readonly ISequencedNotes sequencedNotes;
        [NotNull] private readonly ISignalProviderFactory signalProviderFactory;
        [NotNull] private readonly ITickCalculator tickCalculator;
        [NotNull] private readonly DrumPattern drumPattern;
        [NotNull] private readonly DrumKitSettings drumKit;
        private readonly int sampleRate;

        // The open hi-hat still ringing, so the next closed or open hat can cut it off.
        [CanBeNull] private DrumVoice ringingOpenHat;
        private readonly int previewSamples;

        // Ticks arrive on the clock thread; Stopped can be raised from the UI thread.
        [NotNull] private readonly object voicesLock = new object();
        [NotNull] private readonly Dictionary<ISequencedNote, ISignalProvider> voices = new Dictionary<ISequencedNote, ISignalProvider>();
        private volatile bool isRunning;

        public SequencerPlayer([NotNull] ISignalProviderFactory signalProviderFactory, [NotNull] ISequencedNotes sequencedNotes,
            [NotNull] ISequencerClock clock, [NotNull] ITickCalculator tickCalculator, [NotNull] IAudioSettings audioSettings,
            [NotNull] IAudioOutput output, [NotNull] DrumPattern drumPattern, [NotNull] DrumKitSettings drumKit)
        {
            this.drumPattern = drumPattern;
            this.drumKit = drumKit;
            sampleRate = audioSettings.SampleRate;
            this.signalProviderFactory = signalProviderFactory;
            this.sequencedNotes = sequencedNotes;
            this.clock = clock;
            this.tickCalculator = tickCalculator;
            this.output = output;
            previewSamples = (int) (PreviewSeconds * audioSettings.SampleRate);

            // ReadFully keeps the stream running with silence when no notes are playing.
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(audioSettings.SampleRate, 1)) { ReadFully = true };
            // Master volume, then a limiter so overlapping notes can never clip.
            output.Play(new LimiterSampleProvider(new MasterVolumeSampleProvider(mixer, audioSettings)));

            clock.Tick += OnTick;
            clock.Started += OnClockStarted;
            clock.Stopped += OnClockStopped;
        }

        /// <summary>
        /// The number of notes currently sounding, including ones in their release stage.
        /// </summary>
        public int ActiveVoiceCount => mixer.MixerInputs.Count();

        /// <summary>
        /// Plays a short, self-releasing note alongside whatever is playing. Safe to call from the UI thread.
        /// </summary>
        public void Preview([NotNull] Tone tone)
        {
            ISignalProvider voice = signalProviderFactory.CreateSignalProvider(new PreviewNote(tone));
            voice.NoteOn();
            voice.ReleaseAfter(previewSamples);
            mixer.AddMixerInput(voice);
        }

        /// <summary>
        /// Starts a note that plays until the returned handle is disposed. Safe to call from the UI thread.
        /// </summary>
        [NotNull]
        public IDisposable Hold([NotNull] Tone tone)
        {
            ISignalProvider voice = signalProviderFactory.CreateSignalProvider(new PreviewNote(tone));
            voice.NoteOn();
            mixer.AddMixerInput(voice);
            return new HeldNote(voice);
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
            ReleaseAllNotes();
        }

        private void OnTick(object sender, [NotNull] TickEventArgs e)
        {
            // Stop and Pause also raise a tick to update the position display. Those must not start notes.
            if (!isRunning)
            {
                return;
            }

            if (e.CurrentTick == 0)
            {
                // The loop has wrapped. A note ending exactly at the end of the sequence never sees its
                // ending tick, because that tick is reported as 0, so release everything still sounding.
                ReleaseAllNotes();
            }

            IPosition currentPosition = tickCalculator.CalculatePositionFromTick(e.CurrentTick);

            StartNotes(currentPosition);
            ReleaseNotes(currentPosition);

            int ticksPerSixteenth = Math.Max(1, clock.TicksPerQuarterNote / 4);

            if (e.CurrentTick % ticksPerSixteenth == 0)
            {
                TriggerDrums(e.CurrentTick / ticksPerSixteenth % DrumPattern.StepCount);
            }
        }

        /// <summary>
        /// Plays every drum hit on this step.
        /// </summary>
        private void TriggerDrums(int step)
        {
            foreach (DrumSound sound in DrumPattern.AllSounds)
            {
                byte velocity = drumPattern.VelocityAt(sound, step);

                if (velocity > 0)
                {
                    PlayDrum(sound, velocity);
                }
            }
        }

        /// <summary>
        /// Plays one drum hit now, alongside whatever is playing. Safe to call from the UI thread.
        /// </summary>
        public void PreviewDrum(DrumSound sound, byte velocity)
        {
            PlayDrum(sound, velocity);
        }

        private void PlayDrum(DrumSound sound, byte velocity)
        {
            var voice = new DrumVoice(sound, velocity, sampleRate, (float) drumKit.Volume);

            // On a drum machine the hi-hats share one voice: a new hat cuts off an open one still ringing.
            if (sound == DrumSound.ClosedHat || sound == DrumSound.OpenHat)
            {
                ringingOpenHat?.Choke();
                ringingOpenHat = sound == DrumSound.OpenHat ? voice : null;
            }

            mixer.AddMixerInput(voice);
        }

        private void StartNotes([NotNull] IPosition currentPosition)
        {
            foreach (ISequencedNote note in sequencedNotes.FindNotesFromStartingPosition(currentPosition))
            {
                lock (voicesLock)
                {
                    if (voices.ContainsKey(note))
                    {
                        continue;
                    }

                    ISignalProvider voice = signalProviderFactory.CreateSignalProvider(note);
                    voice.NoteOn();
                    voices.Add(note, voice);
                    mixer.AddMixerInput(voice);
                }
            }
        }

        private void ReleaseNotes([NotNull] IPosition currentPosition)
        {
            foreach (ISequencedNote note in sequencedNotes.FindNotesFromEndingPosition(currentPosition))
            {
                ReleaseNote(note);
            }
        }

        private void ReleaseAllNotes()
        {
            lock (voicesLock)
            {
                foreach (ISignalProvider voice in voices.Values)
                {
                    voice.NoteOff();
                }

                voices.Clear();
            }
        }

        private void ReleaseNote([NotNull] ISequencedNote note)
        {
            lock (voicesLock)
            {
                if (voices.TryGetValue(note, out ISignalProvider voice))
                {
                    // The voice stays in the mixer through its release stage and removes itself once silent.
                    voice.NoteOff();
                    voices.Remove(note);
                }
            }
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
