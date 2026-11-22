using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sequencer.Domain;

namespace Sequencer.Visual
{
    public interface ISequencerNotes
    {
        /// <summary>
        /// The currently selected sequencer notes.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IVisualNote> SelectedNotes { get; }

        /// <summary>
        /// All the notes in the sequencer.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IVisualNote> AllNotes { get; }

        event EventHandler<SelectedNotesEventArgs> SelectedNotesChanged;

        /// <summary>
        /// Raised when a note is added or deleted.
        /// </summary>
        event EventHandler NotesChanged;

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
        void DeleteNote([NotNull] IVisualNote noteToDelete);


        [NotNull]
        [ItemNotNull]
        IEnumerable<IVisualNote> FindAllOtherNotes([NotNull] IEnumerable<IVisualNote> notesToIgnore);

        [NotNull]
        [ItemNotNull]
        IEnumerable<IVisualNote> FindNotesFromStartingPosition([NotNull] IPosition position);

        [NotNull]
        [ItemNotNull]
        IEnumerable<IVisualNote> FindNotesFromEndingPosition([NotNull] IPosition position);

        void NoteStateChanged();
    }
}