using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NAudio.Wave;
using NUnit.Framework;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.Audio.Tests
{
    [TestFixture]
    public class SequencerPlayerTests
    {
        private const int SampleRate = 44100;

        private Mock<ISequencerClock> clock;
        private Mock<ISequencedNotes> notes;
        private Mock<IAudioOutput> output;
        private Mock<ITickCalculator> tickCalculator;
        private Dictionary<ISequencedNote, FakeVoice> voices;
        private SequencerPlayer player;
        private ISampleProvider playedSource;
        private double masterVolume = 1.0;

        private readonly IPosition start = new Position(1, 1, 1);
        private readonly IPosition end = new Position(1, 2, 1);

        [SetUp]
        public void SetUp()
        {
            clock = new Mock<ISequencerClock>();
            notes = new Mock<ISequencedNotes>();
            output = new Mock<IAudioOutput>();
            output.Setup(x => x.Play(It.IsAny<ISampleProvider>())).Callback<ISampleProvider>(source => playedSource = source);
            voices = new Dictionary<ISequencedNote, FakeVoice>();

            notes.Setup(x => x.FindNotesFromStartingPosition(It.IsAny<IPosition>())).Returns(Array.Empty<ISequencedNote>());
            notes.Setup(x => x.FindNotesFromEndingPosition(It.IsAny<IPosition>())).Returns(Array.Empty<ISequencedNote>());

            var factory = new Mock<ISignalProviderFactory>();
            factory.Setup(x => x.CreateSignalProvider(It.IsAny<ISequencedNote>())).Returns<ISequencedNote>(note =>
            {
                var voice = new FakeVoice();
                voices[note] = voice;
                return voice;
            });

            // Tick 0 is the start position, tick 1 the end position.
            tickCalculator = new Mock<ITickCalculator>();
            tickCalculator.Setup(x => x.CalculatePositionFromTick(0)).Returns(start);
            tickCalculator.Setup(x => x.CalculatePositionFromTick(1)).Returns(end);

            var audioSettings = new Mock<IAudioSettings>();
            audioSettings.Setup(x => x.SampleRate).Returns(SampleRate);
            audioSettings.Setup(x => x.MasterVolume).Returns(() => masterVolume);

            player = new SequencerPlayer(factory.Object, notes.Object, clock.Object, tickCalculator.Object, audioSettings.Object, output.Object);
        }

        [TearDown]
        public void TearDown()
        {
            player.Dispose();
        }

        private ISequencedNote AddNote()
        {
            var note = new Mock<ISequencedNote>().Object;
            notes.Setup(x => x.FindNotesFromStartingPosition(start)).Returns(new[] { note });
            notes.Setup(x => x.FindNotesFromEndingPosition(end)).Returns(new[] { note });
            return note;
        }

        private void RaiseStarted() => clock.Raise(x => x.Started += null, EventArgs.Empty);

        private void RaiseStopped() => clock.Raise(x => x.Stopped += null, EventArgs.Empty);

        private void RaiseTick(int tick) => clock.Raise(x => x.Tick += null, new TickEventArgs(tick));

        [Test]
        public void Constructor_StartsTheOutputWithOneMixedStream()
        {
            output.Verify(x => x.Play(It.IsAny<ISampleProvider>()), Times.Once);
            Assert.That(playedSource.WaveFormat.SampleRate, Is.EqualTo(SampleRate));
        }

        [Test]
        public void Tick_AtNoteStart_StartsTheNote()
        {
            ISequencedNote note = AddNote();
            RaiseStarted();

            RaiseTick(0);

            Assert.That(voices[note].NoteOnCount, Is.EqualTo(1));
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(1));
        }

        [Test]
        public void Tick_AtNoteEnd_ReleasesTheNote()
        {
            ISequencedNote note = AddNote();
            RaiseStarted();
            RaiseTick(0);

            RaiseTick(1);

            Assert.That(voices[note].NoteOffCount, Is.EqualTo(1));
        }

        [Test]
        public void Tick_WhenClockIsNotRunning_DoesNotStartNotes()
        {
            // Stop and Pause raise a tick to update the position display.
            AddNote();

            RaiseTick(0);

            Assert.That(voices, Is.Empty);
        }

        [Test]
        public void Tick_ForANoteAlreadySounding_DoesNotStartItAgain()
        {
            // Tick 0 means the loop wrapped, so use a later tick that also maps to the note's start.
            AddNote();
            tickCalculator.Setup(x => x.CalculatePositionFromTick(2)).Returns(start);
            RaiseStarted();

            RaiseTick(2);
            RaiseTick(2);

            Assert.That(player.ActiveVoiceCount, Is.EqualTo(1));
        }

        [Test]
        public void Stopped_ReleasesSoundingNotes()
        {
            ISequencedNote note = AddNote();
            RaiseStarted();
            RaiseTick(0);

            RaiseStopped();

            Assert.That(voices[note].NoteOffCount, Is.EqualTo(1));
        }

        [Test]
        public void Tick_AtLoopStart_ReleasesNotesStillSounding()
        {
            var longNote = new Mock<ISequencedNote>().Object;
            notes.Setup(x => x.FindNotesFromStartingPosition(end)).Returns(new[] { longNote });
            RaiseStarted();
            RaiseTick(1);

            // The note ends at the end of the sequence, which is reported as tick 0 of the next loop.
            RaiseTick(0);

            Assert.That(voices[longNote].NoteOffCount, Is.EqualTo(1));
        }

        [Test]
        public void FinishedVoices_AreRemovedFromTheMixer()
        {
            ISequencedNote note = AddNote();
            RaiseStarted();
            RaiseTick(0);
            voices[note].Finished = true;

            playedSource.Read(new float[64]);

            Assert.That(player.ActiveVoiceCount, Is.EqualTo(0));
        }

        [Test]
        public void Output_IsScaledByTheMasterVolume()
        {
            ISequencedNote note = AddNote();
            RaiseStarted();
            RaiseTick(0);
            voices[note].Level = 0.8f;
            masterVolume = 0.5;

            var buffer = new float[16];
            playedSource.Read(buffer);

            Assert.That(buffer, Is.All.EqualTo(0.4f).Within(1e-6));
        }

        [Test]
        public void Preview_PlaysAShortSelfReleasingNote()
        {
            var tone = new Tone(new Pitch(Note.C, 4), new Velocity(100), start, end);

            player.Preview(tone);

            FakeVoice voice = voices.Values.Single();
            Assert.That(voice.NoteOnCount, Is.EqualTo(1));
            Assert.That(voice.ReleaseAfterSamples, Is.EqualTo((int) (0.15 * SampleRate)));
            Assert.That(player.ActiveVoiceCount, Is.EqualTo(1));
        }

        [Test]
        public void Hold_PlaysUntilTheHandleIsDisposed()
        {
            var tone = new Tone(new Pitch(Note.C, 4), new Velocity(100), start, end);

            IDisposable held = player.Hold(tone);
            FakeVoice voice = voices.Values.Single();
            Assert.That(voice.NoteOnCount, Is.EqualTo(1));
            Assert.That(voice.ReleaseAfterSamples, Is.Null, "Held notes do not release by themselves.");
            Assert.That(voice.NoteOffCount, Is.EqualTo(0));

            held.Dispose();

            Assert.That(voice.NoteOffCount, Is.EqualTo(1));
        }

        [Test]
        public void NotePreviewer_HoldPlaysEvenWithPreviewsSwitchedOff()
        {
            var previewer = new NotePreviewer(player, new SynthSettings { PreviewOnEdit = false });

            previewer.Hold(new Tone(new Pitch(Note.C, 4), new Velocity(100), start, end));

            Assert.That(voices.Count, Is.EqualTo(1));
        }

        [Test]
        public void NotePreviewer_OnlyPreviewsWhenSwitchedOn()
        {
            var settings = new SynthSettings { PreviewOnEdit = false };
            var previewer = new NotePreviewer(player, settings);
            var tone = new Tone(new Pitch(Note.C, 4), new Velocity(100), start, end);

            previewer.Preview(tone);
            Assert.That(voices, Is.Empty);

            settings.PreviewOnEdit = true;
            previewer.Preview(tone);
            Assert.That(voices.Count, Is.EqualTo(1));
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

            public bool Finished { get; set; }

            public float Level { get; set; }

            public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1);

            public float Amplitude { get; set; }

            public float Frequency { get; set; }

            public bool IsFinished => Finished;

            public void NoteOn() => NoteOnCount++;

            public void NoteOff() => NoteOffCount++;

            public int? ReleaseAfterSamples { get; private set; }

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
