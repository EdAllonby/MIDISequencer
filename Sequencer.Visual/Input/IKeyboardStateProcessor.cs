namespace Sequencer.Visual.Input
{
    /// <summary>
    /// The modifier keys currently held. Avalonia has no global keyboard state, so this is kept up to
    /// date from input events.
    /// </summary>
    public interface IKeyboardStateProcessor
    {
        /// <summary>
        /// The platform's command modifier: ⌘ on macOS, Ctrl elsewhere. Adds to or removes from the selection.
        /// </summary>
        bool IsCommandModifierDown { get; }

        /// <summary>
        /// Shift. Locks note dragging to the time axis.
        /// </summary>
        bool IsShiftDown { get; }
    }
}
