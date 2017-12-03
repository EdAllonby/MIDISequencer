using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Midi;
using Sequencer.Shared;
using Sequencer.Utilities;
using Sequencer.ViewModel.Command;

namespace Sequencer.ViewModel
{
    public sealed class SequencerViewModel : ViewModelBase
    {
        [NotNull] private readonly ISequencerClock clock;
        [NotNull] private readonly IMusicalSettings musicalSettings;
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(SequencerViewModel));

        [NotNull] private NoteAction noteAction = NoteAction.Create;
        [NotNull] private IEnumerable<Tone> selectedNotes = new List<Tone>();
        [CanBeNull] private object selectedObject;
        private bool sequencerPlaying;
        private IPosition currentPosition;


        public SequencerViewModel([NotNull] ISequencerClock clock, [NotNull] IMusicalSettings musicalSettings)
        {
            this.clock = clock;
            this.musicalSettings = musicalSettings;
            CurrentPosition = new Position(1, 1, 1);

            clock.Tick += OnTick;

        }

        private void OnTick(object sender, EventArgs e)
        {
            if (clock.Ticks % 6 == 0)
            {
                IPosition nextPosition = CurrentPosition.NextPosition(musicalSettings.TimeSignature);

                // Abstract this. For some reason (yet to be discovered), raising property changes on the UI thread is much, much quicker than letting WPF handle it.
                // I don't really like having this View dependency in the view model, but we'll have to live with it for the moment.
                Application.Current.Dispatcher.Invoke(() => {
                    CurrentPosition = nextPosition;
                });
            }
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

        private bool CanExecuteStopCommand(object obj)
        {
            return sequencerPlaying;
        }

        private void ExecuteStopCommand(object obj)
        {
            SequencerPlaying = false;
            clock.Stop();
        }

        private bool CanExecutePlayCommand(object obj)
        {
            return !sequencerPlaying;
        }

        private void ExecutePlayCommand(object obj)
        {
            SequencerPlaying = true;
            clock.Start();
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}