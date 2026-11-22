using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NAudio.Wave;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Drums;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class SequencerPlayerTests
    {
        private const int SampleRate = 44100;
        private const int TicksPerSixteenth = 24;
        private const int TicksPerSection = 1536;

        private Mock<ISequencerClock> clock;
        private Mock<IAudioOutput> output;
        private Song song;
        private ArrangementState state;
        private MixerSettings mixer;
        private Track synth;
        private Track drums;
        private NoteClip melody;
        private DrumClip beat;
        private List<(Tone Tone, IAudioSettings Instrument, FakeVoice Voice)> created;
        private SequencerPlayer player;
        private ISampleProvider playedSource;
        private TickCalculator ticks;

        [SetUp]
        public void SetUp()
        {
            clock = new Mock<ISequencerClock>();
            clock.Setup(x => x.TicksPerQuarterNote).Returns(96);
            output = new Mock<IAudioOutput>();
            output.Setup(x => x.Play(It.IsAny<ISampleProvider>())).Callback<ISampleProvider>(source => playedSource = source);
            created = new List<(Tone, IAudioSettings, FakeVoice)>();

            var factory = new Mock<ISignalProviderFactory>();
            factory.Setup(x => x.CreateSignalProvider(It.IsAny<ISequencedNote>(), It.IsAny<IAudioSettings>()))
                .Returns<ISequencedNote, IAudioSettings>((note, instrument) =>
                {
                    var voice = new FakeVoice();
                    created.Add((note.Tone, instrument, voice));
                    return voice;
                });

            song = Song.CreateDefault();
            synth = song.Tracks.Single(t => t.Kind == TrackKind.Synth);
            drums = song.Tracks.Single(t => t.Kind == TrackKind.Drums);
            melody = (NoteClip) synth.ClipAt(0);
            beat = (DrumClip) drums.ClipAt(0);
            state = new ArrangementState();
            mixer = new MixerSettings();
            ticks = new TickCalculator(new SequencerSettings());

            player = new SequencerPlayer(factory.Object, song, state, clock.Object, ticks, mixer, output.Object);
        }

        [TearDown]
        public void TearDown()
        {
            player.Dispose();
        }

        private Tone ToneAt(int startSixteenth, int lengthSixteenths, Note note = null) =>
            new Tone(new Pitch(note ?? Note.C, 5), new Velocity(100),
                ticks.CalculatePositionFromTick(startSixteenth * TicksPerSixteenth),
                ticks.CalculatePositionFromTick((startSixteenth + lengthSixteenths) * TicksPerSixteenth));

        private FakeVoice VoiceFor(Tone tone) => created.Single(c => c.Tone == tone).Voice;

        private void RaiseStarted() => clock.Raise(x => x.Started += null, EventArgs.Empty);

        private void RaiseStopped() => clock.Raise(x => x.Stopped += null, EventArgs.Empty);

        private void RaiseTick(int tick) => clock.Raise(x => x.Tick += null, new TickEventArgs(tick));

        private void RaiseSixteenth(int sixteenth) => RaiseTick(sixteenth * TicksPerSixteenth);

        [Test]
        public void Constructor_StartsTheOutputWithOneMixedStream()
        {
            output.Verify(x => x.Play(It.IsAny<ISampleProvider>()), Times.Once);
            Assert.That(playedSource.WaveFormat.SampleRate, Is.EqualTo(SampleRate));
        }

        [Test]
        public void ANote_StartsAndReleasesOnItsTicks()
        {
            Tone tone = ToneAt(2, 4);
            melody.SetTones(new[] { tone });
            RaiseStarted();

            RaiseSixteenth(2);
            Assert.That(VoiceFor(tone).NoteOnCount, Is.EqualTo(1));

            RaiseSixteenth(6);
            Assert.That(VoiceFor(tone).NoteOffCount, Is.EqualTo(1));
        }

        [Test]
        public void NotesAndDrums_DoNotPlayWhenStopped()
        {
            // Stop and Pause raise a tick to update the position display.
            melody.SetTones(new[] { ToneAt(0, 1) });
            beat.Pattern.SetVelocity(DrumSound.Kick, 0, 96);

            RaiseTick(0);

            Assert.That(created, Is.Empty);
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(0));
        }

        [Test]
        public void ANoteAlreadySounding_IsNotStartedAgain()
        {
            melody.SetTones(new[] { ToneAt(2, 4) });
            RaiseStarted();

            RaiseSixteenth(2);
            RaiseSixteenth(2);

            Assert.That(created.Count, Is.EqualTo(1));
        }

        [Test]
        public void Stopped_ReleasesSoundingNotes()
        {
            Tone tone = ToneAt(0, 8);
            melody.SetTones(new[] { tone });
            RaiseStarted();
            RaiseTick(0);

            RaiseStopped();

            Assert.That(VoiceFor(tone).NoteOffCount, Is.EqualTo(1));
        }

        [Test]
        public void TheStartOfASection_ReleasesNotesStillSounding()
        {
            // A note ending at the end of the section is reported as tick 0 of the next.
            Tone tone = ToneAt(60, 4);
            melody.SetTones(new[] { tone });
            RaiseStarted();
            RaiseSixteenth(60);

            RaiseTick(0);

            Assert.That(VoiceFor(tone).NoteOffCount, Is.EqualTo(1));
        }

        [Test]
        public void FinishedVoices_AreRemovedFromTheMixer()
        {
            Tone tone = ToneAt(0, 1);
            melody.SetTones(new[] { tone });
            RaiseStarted();
            RaiseTick(0);
            VoiceFor(tone).Finished = true;

            playedSource.Read(new float[64]);

            Assert.That(player.ActiveVoiceCount, Is.EqualTo(0));
        }

        [Test]
        public void Output_IsScaledByTheMasterVolume()
        {
            Tone tone = ToneAt(0, 4);
            melody.SetTones(new[] { tone });
            RaiseStarted();
            RaiseTick(0);
            VoiceFor(tone).Level = 0.8f;
            mixer.MasterVolume = 0.5;

            // Read past the limiter's look-ahead delay.
            var buffer = new float[400];
            playedSource.Read(buffer);

            Assert.That(buffer.Skip(380), Is.All.EqualTo(0.4f).Within(1e-6));
        }

        [Test]
        public void Notes_UseTheirTracksInstrument()
        {
            Track second = song.AddTrack(TrackKind.Synth);
            var secondClip = (NoteClip) song.CreateClip(second);
            song.Place(second, 0, secondClip);
            Tone first = ToneAt(0, 1);
            Tone other = ToneAt(0, 1, Note.E);
            melody.SetTones(new[] { first });
            secondClip.SetTones(new[] { other });
            RaiseStarted();

            RaiseTick(0);

            Assert.That(created.Single(c => c.Tone == first).Instrument, Is.SameAs(synth.Synth));
            Assert.That(created.Single(c => c.Tone == other).Instrument, Is.SameAs(second.Synth));
        }

        [Test]
        public void SongMode_PlaysEachSectionsClipsInTurn()
        {
            var later = (NoteClip) song.CreateClip(synth);
            song.Place(synth, 1, later);
            Tone inFirst = ToneAt(0, 1);
            Tone inSecond = ToneAt(0, 1, Note.G);
            melody.SetTones(new[] { inFirst });
            later.SetTones(new[] { inSecond });
            state.Mode = PlaybackMode.Song;
            RaiseStarted();

            RaiseTick(0);
            Assert.That(created.Select(c => c.Tone), Is.EqualTo(new[] { inFirst }));

            RaiseTick(TicksPerSection);
            Assert.That(created.Select(c => c.Tone), Is.EqualTo(new[] { inFirst, inSecond }));
        }

        [Test]
        public void SectionMode_LoopsTheChosenSection()
        {
            var later = (NoteClip) song.CreateClip(synth);
            song.Place(synth, 1, later);
            Tone inSecond = ToneAt(0, 1, Note.G);
            later.SetTones(new[] { inSecond });
            melody.SetTones(new[] { ToneAt(0, 1) });
            state.LoopSection = 1;
            RaiseStarted();

            RaiseTick(0);

            Assert.That(created.Select(c => c.Tone), Is.EqualTo(new[] { inSecond }), "Tick 0 of the loop is the second section.");
        }

        [Test]
        public void AClipInTwoSections_PlaysInBoth()
        {
            Tone tone = ToneAt(0, 1);
            melody.SetTones(new[] { tone });
            song.Place(synth, 1, melody);
            state.Mode = PlaybackMode.Song;
            RaiseStarted();

            RaiseTick(0);
            RaiseTick(TicksPerSection);

            Assert.That(created.Count(c => c.Tone == tone), Is.EqualTo(2));
        }

        [Test]
        public void MutedTracks_AreSilent_AndMutingReleasesTheirNotes()
        {
            Tone tone = ToneAt(0, 8);
            melody.SetTones(new[] { tone });
            beat.Pattern.SetVelocity(DrumSound.Kick, 1, 96);
            RaiseStarted();
            RaiseTick(0);

            synth.IsMuted = true;
            drums.IsMuted = true;
            RaiseSixteenth(1);

            Assert.That(VoiceFor(tone).NoteOffCount, Is.EqualTo(1), "Muting releases the sounding note.");
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(1), "No kick: only the releasing note.");
        }

        [Test]
        public void DrumHits_PlayOnTheirSteps()
        {
            beat.Pattern.SetVelocity(DrumSound.Kick, 0, 96);
            beat.Pattern.SetVelocity(DrumSound.Snare, 2, 96);
            RaiseStarted();

            RaiseSixteenth(0);
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(1));

            RaiseSixteenth(1);
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(1), "Nothing on step 1.");

            RaiseSixteenth(2);
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(2));
        }

        [Test]
        public void DrumHits_OnlyPlayOnSixteenths()
        {
            beat.Pattern.SetVelocity(DrumSound.Kick, 0, 96);
            RaiseStarted();

            RaiseTick(5);

            Assert.That(player.ActiveVoiceCount, Is.EqualTo(0));
        }

        [Test]
        public void ClosedHat_ChokesARingingOpenHat()
        {
            beat.Pattern.SetVelocity(DrumSound.OpenHat, 0, 96);
            beat.Pattern.SetVelocity(DrumSound.ClosedHat, 1, 96);
            RaiseStarted();
            RaiseSixteenth(0);
            playedSource.Read(new float[256]);

            RaiseSixteenth(1);
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(2));

            // 1000 samples is about 23 ms: long enough for the choke, shorter than the closed hat.
            playedSource.Read(new float[1000]);
            playedSource.Read(new float[16]);

            Assert.That(player.ActiveVoiceCount, Is.EqualTo(1), "Only the closed hat is left.");
        }

        [Test]
        public void Preview_PlaysAShortSelfReleasingNoteOnTheTracksInstrument()
        {
            Tone tone = ToneAt(0, 1);

            player.Preview(tone, synth);

            Assert.That(VoiceFor(tone).NoteOnCount, Is.EqualTo(1));
            Assert.That(VoiceFor(tone).ReleaseAfterSamples, Is.EqualTo((int) (0.15 * SampleRate)));
            Assert.That(created.Single().Instrument, Is.SameAs(synth.Synth));
        }

        [Test]
        public void Hold_PlaysUntilTheHandleIsDisposed()
        {
            Tone tone = ToneAt(0, 1);

            IDisposable held = player.Hold(tone, synth);
            Assert.That(VoiceFor(tone).ReleaseAfterSamples, Is.Null, "Held notes do not release by themselves.");
            Assert.That(VoiceFor(tone).NoteOffCount, Is.EqualTo(0));

            held.Dispose();

            Assert.That(VoiceFor(tone).NoteOffCount, Is.EqualTo(1));
        }

        [Test]
        public void NotePreviewer_PreviewsOnlyWhenSwitchedOn_ButHoldAlwaysPlays()
        {
            var editing = new EditingSettings { PreviewOnEdit = false };
            var previewer = new NotePreviewer(player, editing, state, song);

            previewer.Preview(ToneAt(0, 1));
            previewer.PreviewDrum(DrumSound.Kick, 96);
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(0));

            previewer.Hold(ToneAt(0, 1));
            previewer.PlayDrum(DrumSound.Kick, 96);
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(2));
        }

        [Test]
        public void NotePreviewer_UsesTheSelectedTracksInstrument()
        {
            Track second = song.AddTrack(TrackKind.Synth);
            state.SelectedTrack = second;
            var previewer = new NotePreviewer(player, new EditingSettings(), state, song);

            previewer.Preview(ToneAt(0, 1));

            Assert.That(created.Single().Instrument, Is.SameAs(second.Synth));
        }

        [Test]
        public void Dispose_DisposesTheOutput()
        {
            player.Dispose();

            output.Verify(x => x.Dispose(), Times.AtLeastOnce);
        }

        private sealed class FakeVoice : ISignalProvider
        {
            public int NoteOnCount { get; private set; }

            public int NoteOffCount { get; private set; }

            public int? ReleaseAfterSamples { get; private set; }

            public bool Finished { get; set; }

            public float Level { get; set; }

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1);

            public float Amplitude { get; set; }

            public float Frequency { get; set; }

            public bool IsFinished => Finished;

            public void NoteOn() => NoteOnCount++;

            public void NoteOff() => NoteOffCount++;

            public void ReleaseAfter(int sampleCount) => ReleaseAfterSamples = sampleCount;

            public int Read(Span<float> buffer)
            {
                if (Finished)
                {
                    return 0;
                }

                buffer.Fill(Level);
                return buffer.Length;
            }
        }
    }
}
