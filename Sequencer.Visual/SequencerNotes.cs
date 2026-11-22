using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Domain.Settings;
using Sequencer.Utilities;

namespace Sequencer.Visual
{
    /// <summary>
    /// The notes in the sequencer. The UI thread edits them while playback reads them from the clock thread,
    /// so every query returns a snapshot taken under a lock.
    /// </summary>
    public class SequencerNotes : ISequencerNotes, ISequencedNotes
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(ISequencerNotes));
        [NotNull] private readonly IMusicalSettings musicalSettings;
        [NotNull] [ItemNotNull] private readonly List<IVisualNote> notes = new List<IVisualNote>();
        [NotNull] private readonly object notesLock = new object();

        public SequencerNotes([NotNull] IMusicalSettings musicalSettings)
        {
            this.musicalSettings = musicalSettings;
        }

        public event EventHandler<SelectedNotesEventArgs> SelectedNotesChanged;

        public IEnumerable<IVisualNote> SelectedNotes => Snapshot(note => note.NoteState == NoteState.Selected);

        public IEnumerable<IVisualNote> AllNotes => Snapshot(note => true);

        IEnumerable<ISequencedNote> ISequencedNotes.AllNotes => AllNotes;

        public void DrawNotes()
        {
            foreach (IVisualNote visualNote in AllNotes)
            {
                visualNote.Draw();
            }
        }

        public void MakeAllUnselected()
        {
            foreach (IVisualNote note in AllNotes)
            {
                note.NoteState = NoteState.Unselected;
            }
        }

        public void AddNote(IVisualNote note)
        {
            note.Draw();

            lock (notesLock)
            {
                notes.Add(note);
            }
        }

        public void DeleteNote(IVisualNote noteToDelete)
        {
            noteToDelete.Remove();

            lock (notesLock)
            {
                notes.Remove(noteToDelete);
            }

            Log.Info($"Visual note [{noteToDelete}] has been deleted.");
        }

        public IVisualNote FindNoteFromPositionAndPitch(IPosition position, Pitch pitch)
        {
            return Snapshot(note => note.StartPosition.IsLessThanOrEqual(position) && note.EndPosition.IsGreaterThan(position) && note.Pitch.Equals(pitch))
                .FirstOrDefault();
        }

        public IVisualNote FindNoteFromStartingPositionAndPitch(IPosition position, Pitch pitch)
        {
            return Snapshot(note => note.StartPosition.Equals(position) && note.Pitch.Equals(pitch)).FirstOrDefault();
        }

        public IVisualNote FindNoteFromEndingPositionAndPitch(IPosition position, Pitch pitch)
        {
            return Snapshot(note =>
                    note.EndPosition.PreviousPosition(musicalSettings.NoteResolution, musicalSettings.TimeSignature, musicalSettings.TicksPerQuarterNote).Equals(position) &&
                    note.Pitch.Equals(pitch))
                .FirstOrDefault();
        }

        public IEnumerable<IVisualNote> FindNotesFromStartingPosition(IPosition position)
        {
            return Snapshot(note => note.StartPosition.Equals(position));
        }

        public IEnumerable<IVisualNote> FindNotesFromEndingPosition(IPosition position)
        {
            return Snapshot(note => note.EndPosition.Equals(position));
        }

        IEnumerable<ISequencedNote> ISequencedNotes.FindNotesFromStartingPosition(IPosition position)
        {
            return FindNotesFromStartingPosition(position);
        }

        IEnumerable<ISequencedNote> ISequencedNotes.FindNotesFromEndingPosition(IPosition position)
        {
            return FindNotesFromEndingPosition(position);
        }

        public IEnumerable<IVisualNote> FindAllOtherNotes(IEnumerable<IVisualNote> notesToIgnore)
        {
            var ignored = new HashSet<IVisualNote>(notesToIgnore);
            return Snapshot(note => !ignored.Contains(note));
        }

        public void NoteStateChanged()
        {
            SelectedNotesChanged?.Invoke(this, new SelectedNotesEventArgs(SelectedNotes));
        }

        [NotNull]
        [ItemNotNull]
        private List<IVisualNote> Snapshot([NotNull] Func<IVisualNote, bool> predicate)
        {
            lock (notesLock)
            {
                return notes.Where(predicate).ToList();
            }
        }
    }
}
