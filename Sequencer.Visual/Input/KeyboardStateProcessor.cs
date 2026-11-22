using System;
using Avalonia.Input;

namespace Sequencer.Visual.Input
{
    public sealed class KeyboardStateProcessor : IKeyboardStateProcessor
    {
        public KeyboardStateProcessor() : this(PlatformCommandModifier)
        {
        }

        public KeyboardStateProcessor(KeyModifiers commandModifier)
        {
            CommandModifier = commandModifier;
        }

        /// <summary>
        /// ⌘ (Meta) on macOS, Ctrl on Windows and Linux.
        /// </summary>
        public static KeyModifiers PlatformCommandModifier => OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;

        public KeyModifiers CommandModifier { get; }

        /// <summary>
        /// Updated by the view on every key and pointer event.
        /// </summary>
        public KeyModifiers CurrentModifiers { get; set; }

        public bool IsCommandModifierDown => CurrentModifiers.HasFlag(CommandModifier);

        public bool IsShiftDown => CurrentModifiers.HasFlag(KeyModifiers.Shift);
    }
}
