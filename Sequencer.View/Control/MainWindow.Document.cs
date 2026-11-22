using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using JetBrains.Annotations;
using Sequencer.Audio;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Settings;
using Sequencer.Midi;

namespace Sequencer.View.Control
{
    /// <summary>
    /// The song as a document: new, open, save, export, the File menu, the window title, and asking before
    /// unsaved changes are lost.
    /// </summary>
    public partial class MainWindow
    {
        private const string UntitledName = "Untitled";

        [CanBeNull] private string documentPath;
        private bool hasUnsavedChanges;
        private bool closeConfirmed;

        // Opening or switching clips changes what the panels show, not the song.
        private int suppressEdits;
        [CanBeNull] private string openClipFingerprint;

        [CanBeNull] private MixerSettings mixerSettings;
        [CanBeNull] private WavExporter wavExporter;
        [CanBeNull] private MidiFileWriter midiFileWriter;

        /// <summary>
        /// The open song's file, or null for a song never saved.
        /// </summary>
        [CanBeNull]
        internal string DocumentPath => documentPath;

        internal bool HasUnsavedChanges => hasUnsavedChanges;

        internal void InitialiseDocument([NotNull] MixerSettings mixer, [NotNull] WavExporter wav, [NotNull] MidiFileWriter midi)
        {
            mixerSettings = mixer;
            wavExporter = wav;
            midiFileWriter = midi;

            song.Changed += (sender, args) => MarkEdited();
            synthViewModel.PropertyChanged += (sender, args) => MarkEdited();
            drumKitViewModel.PropertyChanged += (sender, args) => MarkEdited();
            EffectsPanel.Edited += (sender, args) => MarkEdited();
            viewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(viewModel.BeatsPerMinute))
                {
                    MarkEdited();
                }
            };

            Closing += OnWindowClosing;
            BuildFileMenu();
            SetUnsavedChanges(false);
        }

        /// <summary>
        /// Starts a new song: one synth track and one drum track.
        /// </summary>
        internal void NewSong()
        {
            ReplaceSong(Song.CreateDefault(), 120);
            documentPath = null;
            SetUnsavedChanges(false);
        }

        /// <summary>
        /// Opens a song file. Throws <see cref="FormatException"/> or <see cref="IOException"/> if it can't.
        /// </summary>
        internal void OpenSong([NotNull] string path)
        {
            LoadedSong loaded;

            using (FileStream stream = File.OpenRead(path))
            {
                loaded = SongFile.Load(stream);
            }

            if (loaded.Song.Tracks.Count == 0)
            {
                loaded = new LoadedSong(Song.CreateDefault(), loaded.BeatsPerMinute);
            }

            ReplaceSong(loaded.Song, loaded.BeatsPerMinute);
            documentPath = path;
            SetUnsavedChanges(false);
            Log.Info($"Opened {path}");
        }

        internal void SaveSong([NotNull] string path)
        {
            // The open note clip lives in the editor; make sure the file gets its latest notes.
            openNoteClip?.Close();

            using (FileStream stream = File.Create(path))
            {
                SongFile.Save(song, viewModel.BeatsPerMinute, stream);
            }

            ReopenSelectedClip();
            documentPath = path;
            SetUnsavedChanges(false);
            Log.Info($"Saved {path}");
        }

        internal void ExportMidi([NotNull] string path)
        {
            openNoteClip?.Close();

            using (FileStream stream = File.Create(path))
            {
                midiFileWriter?.Write(song, viewModel.BeatsPerMinute, stream);
            }

            ReopenSelectedClip();
        }

        internal double ExportWav([NotNull] string path)
        {
            openNoteClip?.Close();
            double seconds = wavExporter?.Export(song, viewModel.BeatsPerMinute, mixerSettings ?? new MixerSettings(), path) ?? 0;
            ReopenSelectedClip();
            return seconds;
        }

        private void ReplaceSong([NotNull] Song replacement, int beatsPerMinute)
        {
            if (viewModel.StopSequencer.CanExecute(null))
            {
                viewModel.StopSequencer.Execute(null);
            }

            suppressEdits++;

            try
            {
                CloseOpenClip();
                song.ReplaceWith(replacement);
                viewModel.BeatsPerMinute = beatsPerMinute;
                Select(song.Tracks.First(), 0);
            }
            finally
            {
                suppressEdits--;
            }
        }

        private void ReopenSelectedClip()
        {
            if (arrangement.SelectedTrack != null)
            {
                suppressEdits++;
                Select(arrangement.SelectedTrack, arrangement.SelectedSection);
                suppressEdits--;
            }
        }

        private void MarkEdited()
        {
            if (suppressEdits == 0)
            {
                SetUnsavedChanges(true);
            }
        }

        /// <summary>
        /// Marks the song edited if the open note clip's notes really changed (not just a selection click).
        /// </summary>
        private void CheckOpenClipEdited()
        {
            string fingerprint = OpenClipFingerprint();

            if (fingerprint != openClipFingerprint)
            {
                openClipFingerprint = fingerprint;
                MarkEdited();
            }
        }

        [CanBeNull]
        private string OpenClipFingerprint()
        {
            return openNoteClip == null
                ? null
                : string.Join(";", notes.AllNotes.Select(note => $"{note.Pitch}@{note.StartPosition}-{note.EndPosition}:{note.Velocity.Value}").OrderBy(text => text));
        }

        private void SetUnsavedChanges(bool unsaved)
        {
            hasUnsavedChanges = unsaved;
            string name = documentPath == null ? UntitledName : Path.GetFileNameWithoutExtension(documentPath);

            // The macOS convention: the document's name, and "Edited" while there are unsaved changes.
            Title = unsaved ? $"{name} — Edited" : name;
        }

        private void BuildFileMenu()
        {
            KeyModifiers command = keyboardState.CommandModifier;
            var file = new NativeMenu();
            file.Items.Add(MenuItem("New", new KeyGesture(Key.N, command), () => RunGuarded(ConfirmDiscardThen(NewSong))));
            file.Items.Add(MenuItem("Open…", new KeyGesture(Key.O, command), () => RunGuarded(ConfirmDiscardThenAsync(OpenWithPickerAsync))));
            file.Items.Add(new NativeMenuItemSeparator());
            file.Items.Add(MenuItem("Save", new KeyGesture(Key.S, command), () => RunGuarded(async () => await SaveAsync())));
            file.Items.Add(MenuItem("Save As…", new KeyGesture(Key.S, command | KeyModifiers.Shift), () => RunGuarded(async () => await SaveAsAsync())));
            file.Items.Add(new NativeMenuItemSeparator());
            file.Items.Add(MenuItem("Export MIDI File…", null, () => RunGuarded(ExportMidiWithPickerAsync)));
            file.Items.Add(MenuItem("Export Audio (WAV)…", null, () => RunGuarded(ExportWavWithPickerAsync)));

            var menu = new NativeMenu();
            menu.Items.Add(new NativeMenuItem("File") { Menu = file });
            NativeMenu.SetMenu(this, menu);
        }

        [NotNull]
        private static NativeMenuItem MenuItem([NotNull] string header, [CanBeNull] KeyGesture gesture, [NotNull] Action action)
        {
            var item = new NativeMenuItem(header) { Gesture = gesture };
            item.Click += (sender, args) => action();
            return item;
        }

        private async void RunGuarded([NotNull] Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception error) when (error is IOException || error is FormatException || error is UnauthorizedAccessException)
            {
                Log.Warn($"File operation failed: {error.Message}");
                await ShowMessageAsync("Something went wrong", error.Message);
            }
        }

        [NotNull]
        private Func<Task> ConfirmDiscardThen([NotNull] Action action) => ConfirmDiscardThenAsync(() =>
        {
            action();
            return Task.CompletedTask;
        });

        [NotNull]
        private Func<Task> ConfirmDiscardThenAsync([NotNull] Func<Task> action) => async () =>
        {
            if (await ConfirmUnsavedChangesAsync())
            {
                await action();
            }
        };

        /// <summary>
        /// If there are unsaved changes, asks whether to save them. Returns false if the user cancels.
        /// </summary>
        private async Task<bool> ConfirmUnsavedChangesAsync()
        {
            if (!hasUnsavedChanges)
            {
                return true;
            }

            string name = documentPath == null ? UntitledName : Path.GetFileNameWithoutExtension(documentPath);
            string answer = await AskAsync($"Save changes to \"{name}\"?", "Your changes will be lost if you don't save them.",
                "Save", "Don't Save", "Cancel");

            switch (answer)
            {
                case "Save":
                    return await SaveAsync();
                case "Don't Save":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Closes without asking about unsaved changes.
        /// </summary>
        internal void CloseDiscardingChanges()
        {
            closeConfirmed = true;
            Close();
        }

        private async void OnWindowClosing(object sender, [NotNull] WindowClosingEventArgs e)
        {
            if (closeConfirmed || !hasUnsavedChanges)
            {
                return;
            }

            e.Cancel = true;

            if (await ConfirmUnsavedChangesAsync())
            {
                closeConfirmed = true;
                Close();
            }
        }

        private async Task OpenWithPickerAsync()
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Song",
                AllowMultiple = false,
                FileTypeFilter = new[] { SongFileType }
            });

            string path = files.FirstOrDefault()?.TryGetLocalPath();

            if (path != null)
            {
                OpenSong(path);
            }
        }

        /// <summary>
        /// Saves to the song's file, or asks where if it has never been saved. Returns false if cancelled.
        /// </summary>
        private async Task<bool> SaveAsync()
        {
            if (documentPath != null)
            {
                SaveSong(documentPath);
                return true;
            }

            return await SaveAsAsync();
        }

        private async Task<bool> SaveAsAsync()
        {
            string path = await PickSavePathAsync("Save Song", documentPath == null ? UntitledName : Path.GetFileNameWithoutExtension(documentPath),
                SongFile.Extension, SongFileType);

            if (path == null)
            {
                return false;
            }

            SaveSong(path);
            return true;
        }

        private async Task ExportMidiWithPickerAsync()
        {
            string path = await PickSavePathAsync("Export MIDI File", SuggestedExportName, "mid",
                new FilePickerFileType("MIDI file") { Patterns = new[] { "*.mid" }, AppleUniformTypeIdentifiers = new[] { "public.midi-audio" } });

            if (path != null)
            {
                ExportMidi(path);
            }
        }

        private async Task ExportWavWithPickerAsync()
        {
            string path = await PickSavePathAsync("Export Audio", SuggestedExportName, "wav",
                new FilePickerFileType("WAV audio") { Patterns = new[] { "*.wav" }, AppleUniformTypeIdentifiers = new[] { "com.microsoft.waveform-audio" } });

            if (path != null)
            {
                double seconds = ExportWav(path);
                Log.Info($"Exported {seconds:F1} s of audio to {path}");
            }
        }

        [NotNull]
        private string SuggestedExportName => documentPath == null ? UntitledName : Path.GetFileNameWithoutExtension(documentPath);

        [NotNull]
        private static FilePickerFileType SongFileType => new FilePickerFileType("Sequencer song") { Patterns = new[] { $"*.{SongFile.Extension}" } };

        [ItemCanBeNull]
        private async Task<string> PickSavePathAsync([NotNull] string title, [NotNull] string name, [NotNull] string extension, [NotNull] FilePickerFileType type)
        {
            IStorageFile file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = $"{name}.{extension}",
                DefaultExtension = extension,
                ShowOverwritePrompt = true,
                FileTypeChoices = new[] { type }
            });

            return file?.TryGetLocalPath();
        }

        /// <summary>
        /// A small modal question with up to three buttons; returns the chosen button's text.
        /// </summary>
        [ItemCanBeNull]
        private async Task<string> AskAsync([NotNull] string question, [NotNull] string detail, [NotNull] params string[] buttons)
        {
            var dialog = new Window
            {
                Title = string.Empty,
                Width = 380,
                SizeToContent = SizeToContent.Height,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ShowInTaskbar = false
            };

            var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right };

            for (var i = 0; i < buttons.Length; i++)
            {
                string label = buttons[i];
                var button = new Button { Content = label, IsDefault = i == 0, IsCancel = label == "Cancel" };
                button.Classes.Add("quiet");

                if (i == 0)
                {
                    button.Classes.Add("accent");
                }

                button.Click += (sender, args) => dialog.Close(label);
                row.Children.Add(button);
            }

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 10,
                Children =
                {
                    new TextBlock { Text = question, FontWeight = FontWeight.SemiBold, TextWrapping = TextWrapping.Wrap },
                    new TextBlock { Text = detail, TextWrapping = TextWrapping.Wrap, Classes = { "label" } },
                    new Border { Height = 6 },
                    row
                }
            };

            return await dialog.ShowDialog<string>(this);
        }

        private Task ShowMessageAsync([NotNull] string title, [NotNull] string message) => AskAsync(title, message, "OK");
    }
}
