using JetBrains.Annotations;
using log4net;
using Sequencer.Utilities;
using Sequencer.Visual.Input;

namespace Sequencer.View.Command.MousePointCommand
{
    public class EmptyMousePointCommand : IMousePointNoteCommand
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(EmptyMousePointCommand));

        public void Execute(IMousePoint mousePoint)
        {
            Log.Warn("Null Mouse Point Command Execute.");
        }
    }
}