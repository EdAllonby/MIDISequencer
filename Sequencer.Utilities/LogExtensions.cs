using System;
using JetBrains.Annotations;
using log4net;

namespace Sequencer.Utilities
{
    public static class LogExtensions
    {
        /// <summary>
        /// This is created to aid Jetbrain annotations, specifically the [NotNull] attribute.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [NotNull]
        public static ILog GetLoggerSafe([NotNull] Type type)
        {
            ILog logger = LogManager.GetLogger(type);

            // ReSharper disable once AssignNullToNotNullAttribute
            return logger;
        }
    }
}