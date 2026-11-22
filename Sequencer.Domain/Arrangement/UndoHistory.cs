using System.Collections.Generic;
using JetBrains.Annotations;

namespace Sequencer.Domain.Arrangement
{
    /// <summary>
    /// Undo and redo as a history of song snapshots: after each edit settles, the whole song is recorded
    /// (a few kilobytes), so every kind of edit is undoable without each needing its own undo code.
    /// </summary>
    public sealed class UndoHistory
    {
        public const int Limit = 200;

        [NotNull] [ItemNotNull] private readonly LinkedList<string> undo = new LinkedList<string>();
        [NotNull] [ItemNotNull] private readonly Stack<string> redo = new Stack<string>();
        [CanBeNull] private string current;

        public bool CanUndo => undo.Count > 0;

        public bool CanRedo => redo.Count > 0;

        /// <summary>
        /// Starts again from a song with no history: a new or opened song.
        /// </summary>
        public void Reset([NotNull] string snapshot)
        {
            undo.Clear();
            redo.Clear();
            current = snapshot;
        }

        /// <summary>
        /// Records the song after an edit. Returns false if nothing actually changed.
        /// </summary>
        public bool Record([NotNull] string snapshot)
        {
            if (snapshot == current)
            {
                return false;
            }

            if (current != null)
            {
                undo.AddLast(current);

                if (undo.Count > Limit)
                {
                    undo.RemoveFirst();
                }
            }

            current = snapshot;
            redo.Clear();
            return true;
        }

        /// <summary>
        /// The snapshot to go back to, or null if there's nothing to undo.
        /// </summary>
        [CanBeNull]
        public string Undo()
        {
            if (undo.Count == 0 || current == null)
            {
                return null;
            }

            redo.Push(current);
            current = undo.Last.Value;
            undo.RemoveLast();
            return current;
        }

        /// <summary>
        /// The snapshot to go forward to again, or null if there's nothing to redo.
        /// </summary>
        [CanBeNull]
        public string Redo()
        {
            if (redo.Count == 0 || current == null)
            {
                return null;
            }

            undo.AddLast(current);
            current = redo.Pop();
            return current;
        }
    }
}
