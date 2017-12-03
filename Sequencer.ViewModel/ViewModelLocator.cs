using System.Diagnostics;
using Microsoft.Practices.ServiceLocation;

namespace Sequencer.ViewModel
{
    public class ViewModelLocator
    {
        public SequencerViewModel Sequencer
        {
            get
            {
                Debug.Assert(ServiceLocator.Current != null, "ServiceLocator.Current != null");
                return ServiceLocator.Current.GetInstance<SequencerViewModel>();
            }
        }
    }
}