using System.Collections.Generic;
using System.Linq;
using log4net;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    public sealed class SequencerViewModel : ViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SequencerViewModel));

        private NoteAction noteAction;
        private IEnumerable<Tone> selectedNotes;
        private object selectedObject;

        public SequencerViewModel()
        {
            NoteAction = NoteAction.Create;
        }

        public NoteAction NoteAction
        {
            get { return noteAction; }
            set
            {
                noteAction = value;
                OnPropertyChanged(nameof(NoteAction));
                Log.Info($"Note action set to {NoteAction}");
            }
        }

        public IEnumerable<Tone> SelectedNotes
        {
            get { return selectedNotes; }
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

        public object SelectedObject
        {
            get { return selectedObject; }
            set
            {
                selectedObject = value;
                OnPropertyChanged(nameof(SelectedObject));
            }
        }
    }
}