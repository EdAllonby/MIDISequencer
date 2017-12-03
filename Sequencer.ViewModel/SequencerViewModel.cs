using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Utilities;
using Sequencer.ViewModel.Command;

namespace Sequencer.ViewModel
{
    public sealed class SequencerViewModel : ViewModelBase
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(SequencerViewModel));

        [NotNull] private NoteAction noteAction = NoteAction.Create;
        [NotNull] private IEnumerable<Tone> selectedNotes = new List<Tone>();
        [CanBeNull] private object selectedObject;
        private bool sequencerPlaying;

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
        }

        private bool CanExecutePlayCommand(object obj)
        {
            return !sequencerPlaying;
        }

        private void ExecutePlayCommand(object obj)
        {
            SequencerPlaying = true;
        }
    }
}