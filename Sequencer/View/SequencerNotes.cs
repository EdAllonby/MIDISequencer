using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;

namespace Sequencer.View
{
    /// <summary>
    /// The notes contained in the sequencer.
    /// </summary>
    public class SequencerNotes : ISequencerNotes
    {
        private readonly SequencerSettings sequencerSettings;
        private static readonly ILog Log = LogManager.GetLogger(typeof(ISequencerNotes));

        [NotNull] private readonly List<IVisualNote> notes = new List<IVisualNote>();

        public event EventHandler<IEnumerable<IVisualNote>> SelectedNotesChanged;

        public SequencerNotes(SequencerSettings sequencerSettings)
        {
            this.sequencerSettings = sequencerSettings;
        }

        /// <summary>
        /// The currently selected sequencer notes.
        /// </summary>
        [NotNull]
        public IEnumerable<IVisualNote> SelectedNotes => notes.Where(note => note.NoteState == NoteState.Selected).ToList();

        /// <summary>
        /// All the notes in the sequencer.
        /// </summary>
        [NotNull]
        public IEnumerable<IVisualNote> AllNotes => notes;

        /// <summary>
        /// Draw all the sequencewr notes on the screen.
        /// </summary>
        public void DrawNotes()
        {
            foreach (IVisualNote visualNote in notes)
            {
                visualNote.Draw();
            }
        }

        /// <summary>
        /// Make all of the notes in the sequencer unselected.
        /// </summary>
        public void MakeAllUnselected()
        {
            notes.ForEach(note => note.NoteState = NoteState.Unselected);
        }

        /// <summary>
        /// Add a note to the sequencer.
        /// </summary>
        /// <param name="note">The note to add to the sequencer.</param>
        public void AddNote(IVisualNote note)
        {
            note.Draw();
            notes.Add(note);
        }

        /// <summary>
        /// Delete a note from the sequencer.
        /// </summary>
        /// <param name="noteToDelete">The note to delete from the sequencer.</param>
        public void DeleteNote(IVisualNote noteToDelete)
        {
            if (noteToDelete == null)
            {
                return;
            }

            noteToDelete.Remove();
            notes.Remove(noteToDelete);
            Log.Info($"Visual note [{noteToDelete}] has been deleted.");
        }

        /// <summary>
        /// Gets the <see cref="VisualNote" /> at a position and pitch in the sequencer.
        /// </summary>
        /// <param name="position">The position of the note.</param>
        /// <param name="pitch">The pitch of the note.</param>
        /// <returns>A <see cref="VisualNote" /> if note. Null if note.</returns>
        public IVisualNote FindNoteFromPositionAndPitch(IPosition position, Pitch pitch)
        {
            return notes.FirstOrDefault(DoesPitchAndPositionMatchCurrentNote(position, pitch));
        }
        
        public IVisualNote FindNoteFromStartingPositionAndPitch(IPosition position, Pitch pitch)
        {
            return notes.FirstOrDefault(DoesPitchAndPositionMatchCurrentNoteStartingPosition(position, pitch));
        }

        public IVisualNote FindNoteFromEndingPositionAndPitch(IPosition position, Pitch pitch)
        {
            return notes.FirstOrDefault(DoesPitchAndPositionMatchCurrentNoteEndingPosition(position, pitch));
        }


        public IEnumerable<IVisualNote> FindAllOtherNotes(IEnumerable<IVisualNote> notesToIgnore)
        {
            return notes.Except(notesToIgnore);
        }
        
        private static Func<IVisualNote, bool> DoesPitchAndPositionMatchCurrentNote(IPosition mousePosition, Pitch mousePitch)
        {
            return visualNote => (visualNote.StartPosition.IsLessThanOrEqual(mousePosition)) && (visualNote.EndPosition.IsGreaterThan(mousePosition)) && visualNote.Pitch.Equals(mousePitch);
        }

        private static Func<IVisualNote, bool> DoesPitchAndPositionMatchCurrentNoteStartingPosition(IPosition mousePosition, Pitch mousePitch)
        {
            return visualNote => visualNote.StartPosition.Equals(mousePosition) && visualNote.Pitch.Equals(mousePitch);
        }

        private Func<IVisualNote, bool> DoesPitchAndPositionMatchCurrentNoteEndingPosition(IPosition mousePosition, Pitch mousePitch)
        {
            return visualNote => visualNote.EndPosition.PreviousPosition(sequencerSettings.TimeSignature).Equals(mousePosition) && visualNote.Pitch.Equals(mousePitch);
        }

        public void NoteStateChanged()
        {
            SelectedNotesChanged?.Invoke(this, SelectedNotes);
        }


    }
}