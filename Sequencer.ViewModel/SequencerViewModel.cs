using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Utilities;

namespace Sequencer.ViewModel
{
    public sealed class SequencerViewModel : ViewModelBase
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(SequencerViewModel));
        [NotNull] private readonly ISequencerClock clock;
        [NotNull] private readonly IWpfDispatcher dispatcher;
        [NotNull] private readonly ITickCalculator tickCalculator;
        [NotNull] private IPosition currentPosition = new Position(1, 1, 1);
        [NotNull] private NoteAction noteAction = NoteAction.Create;
        [NotNull] private IEnumerable<Tone> selectedNotes = new List<Tone>();
        [CanBeNull] private object selectedObject;
        private PlayState sequencerPlayState;


        public SequencerViewModel([NotNull] ISequencerClock clock, [NotNull] ITickCalculator tickCalculator, [NotNull] IWpfDispatcher dispatcher)
        {
            this.clock = clock;
            this.tickCalculator = tickCalculator;
            this.dispatcher = dispatcher;
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
                RaisePropertyChanged(nameof(NoteAction));
                RaisePropertyChanged(nameof(Information));

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
                    RaisePropertyChanged(nameof(SelectedNotes));
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
                RaisePropertyChanged(nameof(SequencerPlayState));
                RaisePropertyChanged(nameof(Information));

                Log.Info($"Sequencer play state set to {SequencerPlayState}");
            }
        }

        [NotNull]
        public IPosition CurrentPosition
        {
            [NotNull] get { return currentPosition; }
            [NotNull]
            set
            {
                currentPosition = value;
                RaisePropertyChanged(nameof(CurrentPosition));

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
                RaisePropertyChanged(nameof(SelectedObject));
            }
        }

        public ICommand PlaySequencer => new RelayCommand(ExecutePlayCommand, CanExecutePlayCommand);

        public ICommand PauseSequencer => new RelayCommand(ExecutePauseCommand, CanExecutePauseCommand);

        public ICommand StopSequencer => new RelayCommand(ExecuteStopCommand, CanExecuteStopCommand);

        private void OnTick(object sender, [NotNull] TickEventArgs e)
        {
            if (e.CurrentTick % (clock.TicksPerQuarterNote / 4) == 0)
            {
                IPosition positionAtTick = tickCalculator.CalculatePositionFromTick(clock.Ticks);
                dispatcher.DispatchToWpf(() => CurrentPosition = positionAtTick);
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
    }
}