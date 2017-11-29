using System.Windows;
using System.Windows.Input;
using log4net;
using Sequencer.Command.MousePointCommand;
using Sequencer.Input;

namespace Sequencer.View
{
    public partial class MainWindow
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));
        private readonly IMouseOperator mouseOperator = new MouseOperator(new MouseStateProcessor());

        public MainWindow()
        {
            InitializeComponent();

            Log.Info("Main Window loaded");
        }

        private void SequencerMouseDown(object sender, MouseButtonEventArgs e)
        {
            IMousePoint mouseDownPoint = SequencerMousePosition(e);

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    Sequencer.HandleLeftMouseDown(mouseDownPoint);
                    break;

                case MouseButton.Right:
                    RadialContextMenu.BuildPopup(mouseDownPoint);
                    break;
            }

            Sequencer.CaptureMouse();
            e.Handled = true;
        }

        private void SequencerMouseMoved(object sender, MouseEventArgs e)
        {
            IMousePoint currentMousePosition = SequencerMousePosition(e);

            if (mouseOperator.CanModifyContextMenu)
            {
                RadialContextMenu.SetCursorPosition(currentMousePosition);
            }
            else
            {
                Sequencer.HandleMouseMovement(currentMousePosition);
            }

            e.Handled = true;
        }

        private void SequencerKeyPressed(object sender, KeyEventArgs e)
        {
            Sequencer.HandleKeyPress(e.Key);
        }

        private void SequencerMouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseButton changedButton = e.ChangedButton;

            switch (changedButton)
            {
                case MouseButton.Left:
                    Sequencer.DragSelectionBox.CloseSelectionBox();
                    break;
                case MouseButton.Right:
                    RadialContextMenu.ClosePopup();
                    break;
            }

            Sequencer.ReleaseMouseCapture();
            e.Handled = true;
        }

        private IMousePoint SequencerMousePosition(MouseEventArgs mouseEventArgs)
        {
            Point point = mouseEventArgs.GetPosition(Sequencer);
            return new MousePoint(point);
        }
    }
}