using System;

namespace Sequencer.ViewModel
{
    /// <summary>
    /// Runs work on the UI thread. Clock ticks arrive on a background thread.
    /// </summary>
    public interface IUiDispatcher
    {
        void Dispatch(Action callback);
    }
}
