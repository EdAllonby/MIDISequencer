using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NAudio.Wave;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.Visual;

namespace Sequencer.Audio
{
    [UsedImplicitly]
    public sealed class SequencerPlayer
    {
        [NotNull] private readonly Dictionary<IVisualNote, WaveOut> currentWaveOuts = new Dictionary<IVisualNote, WaveOut>();
        [NotNull] private readonly IMusicalSettings musicalSettings;
        [NotNull] private readonly ISequencerNotes sequencerNotes;
        [NotNull] private readonly ISignalProviderFactory signalProviderFactory;

        public SequencerPlayer([NotNull] ISignalProviderFactory signalProviderFactory,
            [NotNull] ISequencerNotes sequencerNotes, [NotNull] ISequencerClock sequencerClock,
            [NotNull] IMusicalSettings musicalSettings)
        {
            this.signalProviderFactory = signalProviderFactory;
            this.sequencerNotes = sequencerNotes;
            this.musicalSettings = musicalSettings;

            sequencerClock.Tick += OnTick;
            sequencerClock.Stopped += OnClockStopped;
        }

        private void OnClockStopped(object sender, EventArgs e)
        {
            foreach (IVisualNote note in sequencerNotes.AllNotes)
            {
                StopNote(note);
            }
        }

        private void OnTick(object sender, [NotNull] TickEventArgs e)
        {
            var positionFromTick = new TickCalculator(musicalSettings);
            IPosition currentPosition = positionFromTick.CalculatePositionFromTick(e.CurrentTick);

            PlayNotes(currentPosition);
            StopNotes(currentPosition);
        }

        private void PlayNotes([NotNull] IPosition currentPosition)
        {
            IEnumerable<IVisualNote> notesAtStartPosition = sequencerNotes.FindNotesFromStartingPosition(currentPosition);

            foreach (IVisualNote noteAtStartPosition in notesAtStartPosition)
            {
                ISignalProvider signalProvider = signalProviderFactory.CreateSignalProvider(noteAtStartPosition);
                signalProvider.NoteState = true;

                if (!currentWaveOuts.ContainsKey(noteAtStartPosition))
                {
                    var waveOut = new WaveOut();
                    waveOut.Init(signalProvider);
                    waveOut.Play();
                    currentWaveOuts.Add(noteAtStartPosition, waveOut);
                }
            }
        }

        private void StopNotes([NotNull] IPosition currentPosition)
        {
            IEnumerable<IVisualNote> notesAtEndPosition = sequencerNotes.FindNotesFromEndingPosition(currentPosition);

            foreach (IVisualNote noteAtEndPosition in notesAtEndPosition)
            {
                StopNote(noteAtEndPosition);
            }
        }

        private void StopNote([NotNull] IVisualNote noteAtEndPosition)
        {
            bool didFind = currentWaveOuts.TryGetValue(noteAtEndPosition, out WaveOut value);

            if (didFind)
            {
                value.Stop();
                currentWaveOuts.Remove(noteAtEndPosition);
            }
        }
    }
}