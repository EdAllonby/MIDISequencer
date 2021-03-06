﻿using System;
using System.Windows;
using Sequencer.ViewModel;

namespace Sequencer.View
{
    internal class WpfDispatcher : IWpfDispatcher
    {
        public void DispatchToWpf(Action callback)
        {
            Application.Current?.Dispatcher?.Invoke(callback);
        }
    }
}