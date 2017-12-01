using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using log4net;
using Sequencer.Domain;
using Sequencer.Input;

namespace Sequencer.View
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
            IMousePoint mouseDownPoint = SequencerMousePosition(e);

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    Sequencer?.HandleLeftMouseDown(mouseDownPoint);
                    break;

                case MouseButton.Right:
                    RadialContextMenu?.BuildPopup(mouseDownPoint);
                    break;
            }

            Sequencer?.CaptureMouse();
            e.Handled = true;
        }

        private void SequencerMouseMoved([NotNull] object sender, [NotNull] MouseEventArgs e)
        {
            IMousePoint currentMousePosition = SequencerMousePosition(e);

            if (mouseOperator.CanModifyContextMenu)
            {
                RadialContextMenu?.SetCursorPosition(currentMousePosition);
            }
            else
            {
                Sequencer?.HandleMouseMovement(currentMousePosition);
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
    }
}