using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Utilities;
using Sequencer.Visual.Input;

namespace Sequencer.View.Control
{
    public partial class MainWindow
    {
        [NotNull] private static readonly ILog Log = LogExtensions.GetLoggerSafe(typeof(MainWindow));
        [NotNull] private readonly IMouseOperator mouseOperator = new MouseOperator(new MouseStateProcessor());

        public MainWindow()
        {
            InitializeComponent();

            Log.Info("Main Window loaded");
        }

        private void SequencerMouseDown([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    IMousePoint sequencerMousePosition = SequencerMousePosition(e);
                    Sequencer?.HandleLeftMouseDown(sequencerMousePosition);
                    break;

                case MouseButton.Right:
                    IMousePoint sequencerScrollWindowMousePosition = SequencerScrollerMousePosition(e);
                    RadialContextMenu?.BuildPopup(sequencerScrollWindowMousePosition);
                    break;
            }

            Sequencer?.CaptureMouse();
            e.Handled = true;
        }

        private void SequencerMouseMoved([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            if (mouseOperator.CanModifyContextMenu)
            {
                IMousePoint sequencerScrollWindowMousePosition = SequencerScrollerMousePosition(e);
                RadialContextMenu?.SetCursorPosition(sequencerScrollWindowMousePosition);
            }
            else
            {
                IMousePoint sequencerMousePosition = SequencerMousePosition(e);
                Sequencer?.HandleMouseMovement(sequencerMousePosition);
            }

            e.Handled = true;
        }

        private void SequencerKeyPressed([NotNull] object sender, [NotNull] KeyEventArgs e)
        {
            Sequencer?.HandleKeyPress(e.Key);
        }

        private void SequencerMouseUp([NotNull] object sender, [NotNull] MouseButtonEventArgs e)
        {
            MouseButton changedButton = e.ChangedButton;

            switch (changedButton)
            {
                case MouseButton.Left:
                    Sequencer?.DragSelectionBox?.CloseSelectionBox();
                    break;
                case MouseButton.Right:
                    RadialContextMenu?.ClosePopup();
                    break;
            }

            Sequencer?.ReleaseMouseCapture();
            e.Handled = true;
        }

        [NotNull]
        private IMousePoint SequencerMousePosition([NotNull] MouseEventArgs mouseEventArgs)
        {
            Point point = mouseEventArgs.GetPosition(Sequencer);
            return new MousePoint(point);
        }

        [NotNull]
        private IMousePoint SequencerScrollerMousePosition([NotNull] MouseEventArgs mouseEventArgs)
        {
            Point point = mouseEventArgs.GetPosition(SequencerScrollViewer);
            return new MousePoint(point);
        }
    }
}