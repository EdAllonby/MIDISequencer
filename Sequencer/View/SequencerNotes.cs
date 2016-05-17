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
    public class SequencerNotes
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SequencerNotes));

        [NotNull] private readonly List<VisualNote> notes = new List<VisualNote>();

        /// <summary>
        /// The currently selected sequencer notes.
        /// </summary>
        [NotNull]
        public IEnumerable<VisualNote> SelectedNotes => notes.Where(note => note.NoteState == NoteState.Selected).ToList();

        /// <summary>
        /// All the notes in the sequencer.
        /// </summary>
        [NotNull]
        public IEnumerable<VisualNote> All => notes;

        /// <summary>
        /// Draw all the sequencewr notes on the screen.
        /// </summary>
        public void DrawNotes()
        {
            foreach (VisualNote visualNote in notes)
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
        public void AddNote([NotNull] VisualNote note)
        {
            note.Draw();
            notes.Add(note);
        }

        /// <summary>
        /// Delete a note from the sequencer.
        /// </summary>
        /// <param name="noteToDelete">The note to delete from the sequencer.</param>
        public void DeleteNote(VisualNote noteToDelete)
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
        /// <returns>A <see cref="VisualNot" /> if note. Null if note.</returns>
        public VisualNote FindNoteFromPositionAndPitch([NotNull] Position position, [NotNull] Pitch pitch)
        {
            return notes.FirstOrDefault(DoesPitchAndPositionMatchCurrentNote(position, pitch));
        }

        public IEnumerable<VisualNote> FindAllOtherNotes([NotNull] IEnumerable<VisualNote> notesToIgnore)
        {
            return notes.Except(notesToIgnore);
        }

        private static Func<VisualNote, bool> DoesPitchAndPositionMatchCurrentNote(Position mousePosition, Pitch mousePitch)
        {
            return visualNote => (visualNote.StartPosition <= mousePosition) && (visualNote.EndPosition > mousePosition) && visualNote.Pitch.Equals(mousePitch);
        }
    }
}