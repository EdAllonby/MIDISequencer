using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Domain.Arrangement;
using Sequencer.Domain.Settings;
using Sequencer.Midi;
using Sequencer.Utilities;

namespace Sequencer.ViewModel
{
    public sealed class SequencerViewModel : ObservableObject
    {
        public const int MinimumBeatsPerMinute = 30;
        public const int MaximumBeatsPerMinute = 300;
        private const int DefaultBeatsPerMinute = 120;

        // Taps further apart than this start a new count.
        private static readonly TimeSpan TapTimeout = TimeSpan.FromSeconds(2);
        private const int TapsToAverage = 5;

        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(SequencerViewModel));
        [NotNull] private readonly ISequencerClock clock;
        [NotNull] private readonly IUiDispatcher dispatcher;
        [NotNull] private readonly RelayCommand playCommand;
        [NotNull] private readonly RelayCommand pauseCommand;
        [NotNull] private readonly RelayCommand stopCommand;
        [NotNull] private readonly RelayCommand toggleCommand;
        [NotNull] private readonly RelayCommand tapTempoCommand;
        [NotNull] private readonly TimeProvider timeProvider;
        [NotNull] private readonly List<long> tapTimestamps = new List<long>();
        private int beatsPerMinute;
        [CanBeNull] private readonly Song song;
        [NotNull] private readonly ArrangementState arrangement;
        [NotNull] private readonly EditingSettings editing;
        private int songTick;
        [NotNull] private readonly ITickCalculator tickCalculator;
        [NotNull] private IPosition currentPosition = new Position(1, 1, 1);
        [NotNull] private NoteAction noteAction = NoteAction.Create;
        [NotNull] private IEnumerable<Tone> selectedNotes = new List<Tone>();
        [CanBeNull] private SelectedNoteViewModel selectedNote;
        private PlayState sequencerPlayState;

        public SequencerViewModel([NotNull] ISequencerClock clock, [NotNull] ITickCalculator tickCalculator, [NotNull] IUiDispatcher dispatcher,
            [CanBeNull] TimeProvider timeProvider = null, [CanBeNull] Song song = null, [CanBeNull] ArrangementState arrangement = null,
            [CanBeNull] EditingSettings editing = null)
        {
            this.song = song;
            this.arrangement = arrangement ?? new ArrangementState();
            this.editing = editing ?? new EditingSettings();
            this.timeProvider = timeProvider ?? TimeProvider.System;
            this.clock = clock;
            this.tickCalculator = tickCalculator;
            this.dispatcher = dispatcher;

            playCommand = new RelayCommand(ExecutePlayCommand, CanExecutePlayCommand);
            pauseCommand = new RelayCommand(ExecutePauseCommand, CanExecutePauseCommand);
            stopCommand = new RelayCommand(ExecuteStopCommand, CanExecuteStopCommand);
            toggleCommand = new RelayCommand(ExecuteToggleSequencerCommand);
            tapTempoCommand = new RelayCommand(ExecuteTapTempo);

            double clockBeatsPerMinute = clock.BeatsPerMinute;
            beatsPerMinute = clockBeatsPerMinute > 0 ? (int) Math.Round(clockBeatsPerMinute) : DefaultBeatsPerMinute;

            CurrentPosition = new Position(1, 1, 1);
            SequencerPlayState = PlayState.Stop;

            clock.Tick += OnTick;

            if (song != null)
            {
                song.Changed += (sender, args) => UpdateLoopLength();
            }

            this.arrangement.Changed += (sender, args) =>
            {
                UpdateLoopLength();
                OnPropertyChanged(nameof(IsSongMode));
                OnPropertyChanged(nameof(PositionText));
            };
            UpdateLoopLength();
        }

        /// <summary>
        /// Loop one section while working on it, or play the whole song.
        /// </summary>
        public bool IsSongMode
        {
            get => arrangement.Mode == PlaybackMode.Song;
            set => arrangement.Mode = value ? PlaybackMode.Song : PlaybackMode.Section;
        }

        public bool IsSectionMode
        {
            get => !IsSongMode;
            set => IsSongMode = !value;
        }

        /// <summary>
        /// The playback position in the whole song, in ticks (in section mode, within the looping section).
        /// </summary>
        public int SongTick
        {
            get => songTick;
            private set
            {
                songTick = value;
                OnPropertyChanged(nameof(SongTick));
                OnPropertyChanged(nameof(PositionText));
            }
        }

        /// <summary>
        /// Play notes and drum hits as they are added or changed.
        /// </summary>
        public bool PreviewOnEdit
        {
            get => editing.PreviewOnEdit;
            set
            {
                editing.PreviewOnEdit = value;
                OnPropertyChanged(nameof(PreviewOnEdit));
            }
        }

        private int TicksPerSection => clock.TicksPerQuarterNote * 16;

        /// <summary>
        /// The clock loops one section in section mode, and up to the last clip in song mode.
        /// </summary>
        private void UpdateLoopLength()
        {
            if (TicksPerSection <= 0)
            {
                return;
            }

            int sections = IsSongMode && song != null ? song.LengthInSections : 1;
            clock.LoopLengthTicks = sections * TicksPerSection;
            OnPropertyChanged(nameof(IsSectionMode));
        }

        [NotNull]
        public NoteAction NoteAction
        {
            [NotNull] get { return noteAction; }

            [NotNull]
            set
            {
                noteAction = value;
                OnPropertyChanged(nameof(NoteAction));
                OnPropertyChanged(nameof(Information));
                OnPropertyChanged(nameof(IsSelectTool));
                OnPropertyChanged(nameof(IsDrawTool));
                OnPropertyChanged(nameof(IsEraseTool));
                OnPropertyChanged(nameof(ToolHint));

                Log.Info($"Note action set to {NoteAction}");
            }
        }

        [NotNull]
        public string Information
        {
            [NotNull] get { return $"Note Action {NoteAction}, {sequencerPlayState}, {BeatsPerMinute} BPM"; }
        }

        [NotNull]
        public IEnumerable<Tone> SelectedNotes
        {
            [NotNull] get { return selectedNotes; }
            [NotNull]
            set
            {
                selectedNotes = value;

                if (selectedNotes.Count() == 1)
                {
                    SelectedNote = new SelectedNoteViewModel(selectedNotes.First());
                    OnPropertyChanged(nameof(SelectedNotes));
                }
                else
                {
                    SelectedNote = null;
                }
            }
        }

        public PlayState SequencerPlayState
        {
            [NotNull] get { return sequencerPlayState; }
            [NotNull]
            set
            {
                sequencerPlayState = value;
                OnPropertyChanged(nameof(SequencerPlayState));
                OnPropertyChanged(nameof(Information));
                OnPropertyChanged(nameof(IsPlaying));

                // Unlike WPF's CommandManager, nothing re-queries CanExecute automatically.
                playCommand.NotifyCanExecuteChanged();
                pauseCommand.NotifyCanExecuteChanged();
                stopCommand.NotifyCanExecuteChanged();

                Log.Info($"Sequencer play state set to {SequencerPlayState}");
            }
        }

        [NotNull]
        public IPosition CurrentPosition
        {
            [NotNull] get { return currentPosition; }
            [NotNull]
            private set
            {
                currentPosition = value;
                OnPropertyChanged(nameof(CurrentPosition));
                OnPropertyChanged(nameof(PositionText));

                Log.Info($"Sequencer current position set to {CurrentPosition}");
            }
        }

        [CanBeNull]
        public bool IsPlaying => SequencerPlayState == PlayState.Play;

        private bool isDrumTrack;

        /// <summary>
        /// Which track the editor shows: the synth's piano roll, or the drum grid.
        /// </summary>
        public bool IsDrumTrack
        {
            get => isDrumTrack;
            set
            {
                if (isDrumTrack == value)
                {
                    OnPropertyChanged(nameof(IsDrumTrack));
                    OnPropertyChanged(nameof(IsSynthTrack));
                    return;
                }

                isDrumTrack = value;
                OnPropertyChanged(nameof(IsDrumTrack));
                OnPropertyChanged(nameof(IsSynthTrack));
                OnPropertyChanged(nameof(ToolHint));
            }
        }

        public bool IsSynthTrack
        {
            get => !IsDrumTrack;
            set => IsDrumTrack = !value;
        }

        /// <summary>
        /// The playback position as "bar.beat.sixteenth".
        /// </summary>
        [NotNull]
        public string PositionText
        {
            get
            {
                // The bar within the whole song: section 2 starts at bar 5.
                int ticksPerBar = Math.Max(1, clock.TicksPerQuarterNote * 4);
                int barsBefore = TicksPerSection > 0 ? (IsSongMode ? SongTick / TicksPerSection : arrangement.LoopSection) * 4 : 0;
                string local = MusicalFormat.Position(CurrentPosition, TimeSignature.FourFour, 96);
                int separator = local.IndexOf('.');
                return $"{int.Parse(local.Substring(0, separator)) + barsBefore}{local.Substring(separator)}";
            }
        }

        // One toggle per tool, for a segmented control. Setting one to true selects that tool.
        public bool IsSelectTool
        {
            get => NoteAction == NoteAction.Select;
            set => SelectToolIf(value, NoteAction.Select);
        }

        public bool IsDrawTool
        {
            get => NoteAction == NoteAction.Create;
            set => SelectToolIf(value, NoteAction.Create);
        }

        public bool IsEraseTool
        {
            get => NoteAction == NoteAction.Delete;
            set => SelectToolIf(value, NoteAction.Delete);
        }

        /// <summary>
        /// What the current tool does, for the hint bar.
        /// </summary>
        [NotNull]
        public string ToolHint
        {
            get
            {
                string command = OperatingSystem.IsMacOS() ? "⌘" : "Ctrl+";

                if (IsDrumTrack)
                {
                    string alt = OperatingSystem.IsMacOS() ? "⌥" : "Alt+";
                    return $"Click a step to add or remove a hit · Drag to paint · {alt}click for an accent · Click a sound's name to hear it";
                }

                if (NoteAction == NoteAction.Select)
                {
                    return $"Click a note to select it · Drag to move, or drag an edge to resize · Drag across empty space to select several · {command}click to add or remove";
                }

                if (NoteAction == NoteAction.Delete)
                {
                    return "Click a note to erase it";
                }

                return "Click to add a note · Drag right to make it longer";
            }
        }

        public SelectedNoteViewModel SelectedNote
        {
            get => selectedNote;
            private set
            {
                selectedNote = value;
                OnPropertyChanged(nameof(SelectedNote));
            }
        }

        public ICommand PlaySequencer => playCommand;

        public ICommand PauseSequencer => pauseCommand;

        public ICommand StopSequencer => stopCommand;

        public ICommand ToggleSequencer => toggleCommand;

        /// <summary>
        /// Sets the tempo from the average interval between recent taps.
        /// </summary>
        public ICommand TapTempo => tapTempoCommand;

        public int BeatsPerMinute
        {
            get => beatsPerMinute;
            set
            {
                int clamped = Math.Clamp(value, MinimumBeatsPerMinute, MaximumBeatsPerMinute);

                if (clamped == beatsPerMinute)
                {
                    // Still notify, so a view showing an out-of-range value snaps back to the clamped one.
                    OnPropertyChanged(nameof(BeatsPerMinute));
                    return;
                }

                beatsPerMinute = clamped;
                clock.BeatsPerMinute = clamped;
                OnPropertyChanged(nameof(BeatsPerMinute));
                OnPropertyChanged(nameof(Information));
                Log.Info($"Tempo set to {clamped} BPM");
            }
        }

        private void OnTick(object sender, [NotNull] TickEventArgs e)
        {
            if (e.CurrentTick % (clock.TicksPerQuarterNote / 4) == 0)
            {
                // Editors show the position within the section; the readout and arrangement use the song position.
                int ticksPerSection = TicksPerSection > 0 ? TicksPerSection : int.MaxValue;
                int tickInSection = e.CurrentTick % ticksPerSection;
                IPosition positionAtTick = tickCalculator.CalculatePositionFromTick(tickInSection);
                int tickInSong = IsSongMode ? e.CurrentTick : arrangement.LoopSection * TicksPerSection + tickInSection;
                dispatcher.Dispatch(() =>
                {
                    CurrentPosition = positionAtTick;
                    SongTick = tickInSong;
                });
            }
        }

        private bool CanExecuteStopCommand()
        {
            return SequencerPlayState != PlayState.Stop;
        }

        private void ExecuteStopCommand()
        {
            clock.Stop();
            SequencerPlayState = PlayState.Stop;
        }

        private bool CanExecutePlayCommand()
        {
            return SequencerPlayState != PlayState.Play;
        }

        private void ExecutePlayCommand()
        {
            clock.Start();
            SequencerPlayState = PlayState.Play;
        }

        private void ExecutePauseCommand()
        {
            clock.Pause();
            SequencerPlayState = PlayState.Pause;
        }

        private bool CanExecutePauseCommand()
        {
            return SequencerPlayState == PlayState.Play;
        }

        private void SelectToolIf(bool selected, [NotNull] NoteAction tool)
        {
            if (selected)
            {
                NoteAction = tool;
            }
            else
            {
                // A segmented control always has one tool selected: unchecking the current one re-checks it.
                OnPropertyChanged(nameof(IsSelectTool));
                OnPropertyChanged(nameof(IsDrawTool));
                OnPropertyChanged(nameof(IsEraseTool));
            }
        }

        private void ExecuteTapTempo()
        {
            long now = timeProvider.GetTimestamp();

            if (tapTimestamps.Count > 0 && timeProvider.GetElapsedTime(tapTimestamps[tapTimestamps.Count - 1], now) > TapTimeout)
            {
                tapTimestamps.Clear();
            }

            tapTimestamps.Add(now);

            if (tapTimestamps.Count > TapsToAverage)
            {
                tapTimestamps.RemoveAt(0);
            }

            if (tapTimestamps.Count >= 2)
            {
                TimeSpan span = timeProvider.GetElapsedTime(tapTimestamps[0], now);
                double secondsPerBeat = span.TotalSeconds / (tapTimestamps.Count - 1);
                BeatsPerMinute = (int) Math.Round(60.0 / secondsPerBeat);
            }
        }

        private void ExecuteToggleSequencerCommand()
        {
            if (SequencerPlayState == PlayState.Play)
            {
                ExecutePauseCommand();
            }
            else
            {
                ExecutePlayCommand();
            }
        }
    }
}