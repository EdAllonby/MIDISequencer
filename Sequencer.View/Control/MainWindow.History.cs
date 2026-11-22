using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using JetBrains.Annotations;
using Sequencer.Domain.Arrangement;
using Track = Sequencer.Domain.Arrangement.Track;

namespace Sequencer.View.Control
{
    /// <summary>
    /// Undo and redo: once edits pause for a moment, the song is recorded in the history, so a slider drag or a
    /// burst of typing is one step. Undoing keeps the track and section you're on, and doesn't stop playback.
    /// </summary>
    public partial class MainWindow
    {
        private static readonly TimeSpan SettleTime = TimeSpan.FromMilliseconds(400);
        private static readonly TimeSpan DuplicateWindow = TimeSpan.FromMilliseconds(80);

        [NotNull] private readonly UndoHistory history = new UndoHistory();
        [NotNull] private readonly Stopwatch sinceLastUndoRequest = Stopwatch.StartNew();
        [CanBeNull] private DispatcherTimer settleTimer;
        [CanBeNull] private string savedSnapshot;
        [CanBeNull] private NativeMenuItem undoItem;
        [CanBeNull] private NativeMenuItem redoItem;
        private bool lastUndoRequestFromMenu;

        internal bool CanUndo => history.CanUndo || settleTimer?.IsEnabled == true;

        internal bool CanRedo => history.CanRedo && settleTimer?.IsEnabled != true;

        /// <summary>
        /// Goes back one step. Edits still settling are recorded first, so they're what gets undone.
        /// </summary>
        internal void Undo()
        {
            RecordPendingEdit();
            Restore(history.Undo());
        }

        internal void Redo()
        {
            RecordPendingEdit();
            Restore(history.Redo());
        }

        /// <summary>
        /// Records edits now rather than waiting for them to settle.
        /// </summary>
        internal void RecordPendingEdit()
        {
            if (settleTimer?.IsEnabled != true)
            {
                return;
            }

            settleTimer.Stop();
            history.Record(Snapshot());
            UpdateUndoMenu();
        }

        /// <summary>
        /// Called on every edit: (re)starts the wait before the song is recorded.
        /// </summary>
        private void ScheduleRecord()
        {
            if (settleTimer == null)
            {
                settleTimer = new DispatcherTimer { Interval = SettleTime };
                settleTimer.Tick += (sender, args) => RecordPendingEdit();
            }

            settleTimer.Stop();
            settleTimer.Start();
            UpdateUndoMenu();
        }

        /// <summary>
        /// Starts the history afresh (a new or opened song) and remembers this as the saved state.
        /// </summary>
        private void ResetHistory()
        {
            settleTimer?.Stop();
            string snapshot = Snapshot();
            history.Reset(snapshot);
            savedSnapshot = snapshot;
            UpdateUndoMenu();
        }

        /// <summary>
        /// After saving: records the song and remembers it as the saved state.
        /// </summary>
        private void MarkSaved()
        {
            settleTimer?.Stop();
            string snapshot = Snapshot();
            history.Record(snapshot);
            savedSnapshot = snapshot;
            UpdateUndoMenu();
        }

        [NotNull]
        private string Snapshot()
        {
            var stream = new MemoryStream();
            SongFile.Save(song, viewModel.BeatsPerMinute, stream);
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private void Restore([CanBeNull] string snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            int trackIndex = arrangement.SelectedTrack == null ? 0 : Math.Max(0, song.Tracks.ToList().IndexOf(arrangement.SelectedTrack));
            int section = arrangement.SelectedSection;
            LoadedSong loaded = SongFile.Load(new MemoryStream(Encoding.UTF8.GetBytes(snapshot)));
            suppressEdits++;

            try
            {
                CloseOpenClip();
                song.ReplaceWith(loaded.Song);
                viewModel.BeatsPerMinute = loaded.BeatsPerMinute;
                Track track = song.Tracks[Math.Min(trackIndex, song.Tracks.Count - 1)];
                Select(track, section);
                Arrangement.InvalidateVisual();
            }
            finally
            {
                suppressEdits--;
            }

            // Undoing back to the saved song means there's nothing to save.
            SetUnsavedChanges(snapshot != savedSnapshot);
            UpdateUndoMenu();
        }

        private void AddEditMenu([NotNull] NativeMenu menu)
        {
            KeyModifiers command = keyboardState.CommandModifier;
            var edit = new NativeMenu();
            undoItem = MenuItem("Undo", new KeyGesture(Key.Z, command), () => RequestUndo(true, false));
            redoItem = MenuItem("Redo", new KeyGesture(Key.Z, command | KeyModifiers.Shift), () => RequestUndo(true, true));
            edit.Items.Add(undoItem);
            edit.Items.Add(redoItem);
            menu.Items.Add(new NativeMenuItem("Edit") { Menu = edit });
            UpdateUndoMenu();
        }

        /// <summary>
        /// ⌘Z / Ctrl+Z, ⇧⌘Z / Ctrl+Shift+Z or Ctrl+Y. Returns true if the key was undo or redo.
        /// </summary>
        private bool HandleUndoKeys([NotNull] KeyEventArgs e)
        {
            KeyModifiers command = keyboardState.CommandModifier;
            bool undo = e.Key == Key.Z && e.KeyModifiers == command;
            bool redo = (e.Key == Key.Z && e.KeyModifiers == (command | KeyModifiers.Shift)) ||
                        (e.Key == Key.Y && e.KeyModifiers == KeyModifiers.Control && command == KeyModifiers.Control);

            if (!undo && !redo)
            {
                return false;
            }

            RequestUndo(false, redo);
            e.Handled = true;
            return true;
        }

        /// <summary>
        /// Where a platform delivers the shortcut to both the menu and the window, only the first counts.
        /// </summary>
        private void RequestUndo(bool fromMenu, bool redo)
        {
            bool duplicate = fromMenu != lastUndoRequestFromMenu && sinceLastUndoRequest.Elapsed < DuplicateWindow;
            lastUndoRequestFromMenu = fromMenu;
            sinceLastUndoRequest.Restart();

            if (duplicate)
            {
                return;
            }

            if (redo)
            {
                Redo();
            }
            else
            {
                Undo();
            }
        }

        private void UpdateUndoMenu()
        {
            if (undoItem != null)
            {
                undoItem.IsEnabled = CanUndo;
            }

            if (redoItem != null)
            {
                redoItem.IsEnabled = CanRedo;
            }
        }
    }
}
