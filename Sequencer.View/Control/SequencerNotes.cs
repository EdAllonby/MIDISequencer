using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Shared;
using Sequencer.Utilities;

namespace Sequencer.View.Control
{
    /// <summary>
    /// The notes contained in the sequencer.
    /// </summary>
    public class SequencerNotes : ISequencerNotes
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(ISequencerNotes));

        [NotNull] [ItemNotNull] private readonly List<IVisualNote> notes = new List<IVisualNote>();
        [NotNull] private readonly SequencerSettings sequencerSettings;

        public SequencerNotes([NotNull] SequencerSettings sequencerSettings)
        {
            this.sequencerSettings = sequencerSettings;
        }

        public event EventHandler<IEnumerable<IVisualNote>> SelectedNotesChanged;

        /// <summary>
        /// The currently selected sequencer notes.
        /// </summary>
        public IEnumerable<IVisualNote> SelectedNotes => notes.Where(note => note.NoteState == NoteState.Selected).ToList();

        /// <summary>
        /// All the notes in the sequencer.
        /// </summary>
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
            notes.ForEach(note =>
            {
                if (note != null)
                {
                    note.NoteState = NoteState.Unselected;
                }
            });
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

        public void NoteStateChanged()
        {
            SelectedNotesChanged?.Invoke(this, SelectedNotes);
        }

        [NotNull]
        private static Func<IVisualNote, bool> DoesPitchAndPositionMatchCurrentNote([NotNull] IPosition mousePosition, [NotNull] Pitch mousePitch)
        {
            return visualNote => visualNote != null && visualNote.StartPosition.IsLessThanOrEqual(mousePosition) && visualNote.EndPosition.IsGreaterThan(mousePosition) && visualNote.Pitch.Equals(mousePitch);
        }

        [NotNull]
        private static Func<IVisualNote, bool> DoesPitchAndPositionMatchCurrentNoteStartingPosition([NotNull] IPosition mousePosition, [NotNull] Pitch mousePitch)
        {
            return visualNote => visualNote != null && visualNote.StartPosition.Equals(mousePosition) && visualNote.Pitch.Equals(mousePitch);
        }

        [NotNull]
        private Func<IVisualNote, bool> DoesPitchAndPositionMatchCurrentNoteEndingPosition([NotNull] IPosition mousePosition, [NotNull] Pitch mousePitch)
        {
            return visualNote => visualNote != null && visualNote.EndPosition.PreviousPosition(sequencerSettings.TimeSignature).Equals(mousePosition) && visualNote.Pitch.Equals(mousePitch);
        }
    }
}