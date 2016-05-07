using System.Windows.Controls;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;

namespace Sequencer
{
    public sealed class VisualNote
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VisualNote));

        [NotNull] private readonly NoteDrawer noteDrawer;
        private NoteState noteState = NoteState.Selected;

        public VisualNote([NotNull] SequencerSettings sequencerSettings, [NotNull] Position startPosition, [NotNull] Position endPosition, [NotNull] Pitch pitch)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
            Pitch = pitch;
            noteDrawer = new NoteDrawer(sequencerSettings);
        }

        public Pitch Pitch { get; }

        /// <summary>
        /// The note's starting position.
        /// </summary>
        public Position StartPosition { get; }

        /// <summary>
        /// The note's ending position.
        /// </summary>
        public Position EndPosition { get; private set; }

        /// <summary>
        /// The current state of the note.
        /// </summary>
        public NoteState NoteState
        {
            get { return noteState; }
            set
            {
                noteState = value;
                noteDrawer.SetNoteColour(noteState);
            }
        }

        public void Draw(SequencerDimensionsCalculator sequencerDimensionsCalculator, Canvas sequencer)
        {
            Log.InfoFormat("Drawing note with start position {0} and end position {1}", StartPosition, EndPosition);
            noteDrawer.DrawNote(Pitch, StartPosition, EndPosition, noteState, sequencerDimensionsCalculator.NoteHeight, sequencerDimensionsCalculator.BeatWidth, sequencer);
        }

        public void UpdateNoteLength(TimeSignature timeSignature, Position newEndPosition, double beatWidth)
        {
            if (newEndPosition >= StartPosition)
            {
                Log.InfoFormat("Updating note length with start position {0} to end position {1}", StartPosition, EndPosition);
                EndPosition = newEndPosition;
                noteDrawer.UpdateLength(timeSignature, StartPosition, newEndPosition, beatWidth);
            }
        }

        public void Remove(Canvas sequencer)
        {
            noteDrawer.RemoveNote(sequencer);
        }
        
        public override string ToString()
        {
            return $"Pitch: {Pitch}, Start Position: {StartPosition}, End Position: {EndPosition}";
        }
    }
}