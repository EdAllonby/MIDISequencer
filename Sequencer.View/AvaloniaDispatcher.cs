using System;
using Avalonia.Threading;
using Sequencer.ViewModel;

namespace Sequencer.View
{
    internal sealed class AvaloniaDispatcher : IUiDispatcher
    {
        public void Dispatch(Action callback)
        {
            Dispatcher.UIThread.Post(callback);
        }
    }
}
