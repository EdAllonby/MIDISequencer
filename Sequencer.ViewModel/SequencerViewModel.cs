using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Utilities;

namespace Sequencer.ViewModel
{
    public sealed class SequencerViewModel : ObservableObject
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(SequencerViewModel));
        [NotNull] private readonly ISequencerClock clock;
        [NotNull] private readonly IUiDispatcher dispatcher;
        [NotNull] private readonly RelayCommand playCommand;
        [NotNull] private readonly RelayCommand pauseCommand;
        [NotNull] private readonly RelayCommand stopCommand;
        [NotNull] private readonly RelayCommand toggleCommand;
        [NotNull] private readonly ITickCalculator tickCalculator;
        [NotNull] private IPosition currentPosition = new Position(1, 1, 1);
        [NotNull] private NoteAction noteAction = NoteAction.Create;
        [NotNull] private IEnumerable<Tone> selectedNotes = new List<Tone>();
        [CanBeNull] private object selectedObject;
        private PlayState sequencerPlayState;

        public SequencerViewModel([NotNull] ISequencerClock clock, [NotNull] ITickCalculator tickCalculator, [NotNull] IUiDispatcher dispatcher)
        {
            this.clock = clock;
            this.tickCalculator = tickCalculator;
            this.dispatcher = dispatcher;

            playCommand = new RelayCommand(ExecutePlayCommand, CanExecutePlayCommand);
            pauseCommand = new RelayCommand(ExecutePauseCommand, CanExecutePauseCommand);
            stopCommand = new RelayCommand(ExecuteStopCommand, CanExecuteStopCommand);
            toggleCommand = new RelayCommand(ExecuteToggleSequencerCommand);

            CurrentPosition = new Position(1, 1, 1);
            SequencerPlayState = PlayState.Stop;

            clock.Tick += OnTick;
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

                Log.Info($"Note action set to {NoteAction}");
            }
        }

        [NotNull]
        public string Information
        {
            [NotNull] get { return $"Note Action {NoteAction}, {sequencerPlayState}"; }
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
                    SelectedObject = SelectedNotes.FirstOrDefault();
                    OnPropertyChanged(nameof(SelectedNotes));
                }
                else
                {
                    SelectedObject = null;
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

                Log.Info($"Sequencer current position set to {CurrentPosition}");
            }
        }

        [CanBeNull]
        public object SelectedObject
        {
            get => selectedObject;
            set
            {
                selectedObject = value;
                OnPropertyChanged(nameof(SelectedObject));
            }
        }

        public ICommand PlaySequencer => playCommand;

        public ICommand PauseSequencer => pauseCommand;

        public ICommand StopSequencer => stopCommand;

        public ICommand ToggleSequencer => toggleCommand;

        private void OnTick(object sender, [NotNull] TickEventArgs e)
        {
            if (e.CurrentTick % (clock.TicksPerQuarterNote / 4) == 0)
            {
                IPosition positionAtTick = tickCalculator.CalculatePositionFromTick(e.CurrentTick);
                dispatcher.Dispatch(() => CurrentPosition = positionAtTick);
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