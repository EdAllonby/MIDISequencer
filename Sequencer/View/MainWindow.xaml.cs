﻿using System.Windows;
using System.Windows.Input;
using log4net;

namespace Sequencer.View
{
    public partial class MainWindow
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MainWindow));


        public MainWindow()
        {
            InitializeComponent();

            Log.Info("Main Window loaded");
        }

        private void SequencerMouseDown(object sender, MouseButtonEventArgs e)
        {
            Point mouseDownPoint = SequencerMousePosition(e);

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
            Point currentMousePosition = SequencerMousePosition(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Sequencer.HandleMouseMovement(currentMousePosition);
                return;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                RadialContextMenu.SetCursorPosition(currentMousePosition);
                return;
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

        private Point SequencerMousePosition(MouseEventArgs mouseEventArgs)
        {
            return mouseEventArgs.GetPosition(Sequencer);
        }
    }
}