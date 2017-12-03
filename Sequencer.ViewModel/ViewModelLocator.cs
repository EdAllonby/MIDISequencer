using System;
using System.Diagnostics;
using JetBrains.Annotations;
using log4net;
using Microsoft.Practices.ServiceLocation;
using Sequencer.Utilities;

namespace Sequencer.ViewModel
{
    public class ViewModelLocator
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(ViewModelLocator));

        public SequencerViewModel Sequencer
        {
            get
            {
                try
                {
                    Debug.Assert(ServiceLocator.Current != null, "ServiceLocator.Current != null");
                    return ServiceLocator.Current.GetInstance<SequencerViewModel>();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    throw;
                }
            }
        }
    }
}