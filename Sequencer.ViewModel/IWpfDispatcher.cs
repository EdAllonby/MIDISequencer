using System;

namespace Sequencer.ViewModel
{
    public interface IWpfDispatcher
    {
        void DispatchToWpf(Action callback);
    }
}
