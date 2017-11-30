using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.View
{
    public interface ISequencerNotes
    {
        event EventHandler<IEnumerable<IVisualNote>> SelectedNotesChanged;

        /// <summary>
        /// The currently selected sequencer notes.
        /// </summary>
        IEnumerable<IVisualNote> SelectedNotes { get; }

        /// <summary>
        /// All the notes in the sequencer.
        /// </summary>
        IEnumerable<IVisualNote> AllNotes { get; }

        /// <summary>
        /// Draw all the sequencewr notes on the screen.
        /// </summary>
        void DrawNotes();

        /// <summary>
        /// Make all of the notes in the sequencer unselected.
        /// </summary>
        void MakeAllUnselected();

        /// <summary>
        /// Add a note to the sequencer.
        /// </summary>
        /// <param name="note">The note to add to the sequencer.</param>
        void AddNote([NotNull] IVisualNote note);

        /// <summary>
        /// Delete a note from the sequencer.
        /// </summary>
        /// <param name="noteToDelete">The note to delete from the sequencer.</param>
        void DeleteNote(IVisualNote noteToDelete);

        /// <summary>
        /// Gets the <see cref="IVisualNote" /> at a position and pitch in the sequencer.
        /// </summary>
        /// <param name="position">The position of the note.</param>
        /// <param name="pitch">The pitch of the note.</param>
        /// <returns>A <see cref="VisualNote" /> if note. Null if note.</returns>
        IVisualNote FindNoteFromPositionAndPitch([NotNull] IPosition position, [NotNull] Pitch pitch);

        IVisualNote FindNoteFromStartingPositionAndPitch(IPosition position, Pitch pitch);
        IVisualNote FindNoteFromEndingPositionAndPitch(IPosition position, Pitch pitch);
        IEnumerable<IVisualNote> FindAllOtherNotes([NotNull] IEnumerable<IVisualNote> notesToIgnore);
        void NoteStateChanged();
    }
}