namespace Sequencer.Visual.Input
{
    /// <summary>
    /// The mouse buttons currently held. Avalonia has no global mouse state, so this is kept up to date
    /// from pointer events.
    /// </summary>
    public sealed class MouseOperator : IMouseOperator
    {
        public bool IsLeftButtonPressed { get; set; }

        /// <summary>
        /// The secondary button: right-click, a two-finger trackpad click, or Ctrl-click on macOS.
        /// </summary>
        public bool IsContextButtonPressed { get; set; }

        public bool CanModifyNote => IsLeftButtonPressed && !IsContextButtonPressed;

        public bool CanModifyContextMenu => IsContextButtonPressed;
    }
}
