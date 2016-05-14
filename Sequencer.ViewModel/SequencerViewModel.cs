using log4net;
using Sequencer.Domain;

namespace Sequencer.ViewModel
{
    public sealed class SequencerViewModel : ViewModel
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SequencerViewModel));

        private NoteAction noteAction;

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
    }
}