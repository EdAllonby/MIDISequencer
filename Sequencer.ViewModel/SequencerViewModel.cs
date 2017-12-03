using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Shared;
using Sequencer.Utilities;

namespace Sequencer.ViewModel
{
    public sealed class SequencerViewModel : ViewModelBase
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(SequencerViewModel));
        [NotNull] private readonly ISequencerClock clock;
        [NotNull] private readonly IWpfDispatcher dispatcher;
        [NotNull] private readonly IMusicalSettings musicalSettings;
        [NotNull] private readonly ITickCalculator tickCalculator;
        private IPosition currentPosition;

        [NotNull] private NoteAction noteAction = NoteAction.Create;
        [NotNull] private IEnumerable<Tone> selectedNotes = new List<Tone>();
        [CanBeNull] private object selectedObject;
        private bool sequencerPlaying;


        public SequencerViewModel([NotNull] ISequencerClock clock, [NotNull] IMusicalSettings musicalSettings, [NotNull] ITickCalculator tickCalculator, [NotNull] IWpfDispatcher dispatcher)
        {
            this.clock = clock;
            this.musicalSettings = musicalSettings;
            this.tickCalculator = tickCalculator;
            this.dispatcher = dispatcher;
            CurrentPosition = new Position(1, 1, 1);

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
            [NotNull]
            get
            {
                string playState = SequencerPlaying ? "Playing" : "Stopped";
                return $"Note Action {NoteAction}, Currently {playState}";
            }
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

        public bool SequencerPlaying
        {
            [NotNull] get { return sequencerPlaying; }
            [NotNull]
            set
            {
                sequencerPlaying = value;
                RaisePropertyChanged(nameof(SequencerPlaying));
                RaisePropertyChanged(nameof(Information));

                Log.Info($"Sequencer play state set to {SequencerPlaying}");
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

        public ICommand StopSequencer => new RelayCommand(ExecuteStopCommand, CanExecuteStopCommand);

        private void OnTick(object sender, EventArgs e)
        {
            if (clock.Ticks % 6 == 0)
            {
                IPosition positionAtTick = tickCalculator.CalculatePositionFromTick(clock.Ticks, clock.TicksPerQuarterNote);
                dispatcher.DispatchToWpf(() => CurrentPosition = positionAtTick);
            }
        }

        private bool CanExecuteStopCommand()
        {
            return sequencerPlaying;
        }

        private void ExecuteStopCommand()
        {
            SequencerPlaying = false;
            clock.Stop();
        }

        private bool CanExecutePlayCommand()
        {
            return !sequencerPlaying;
        }

        private void ExecutePlayCommand()
        {
            SequencerPlaying = true;
            clock.Start();
        }
    }
}