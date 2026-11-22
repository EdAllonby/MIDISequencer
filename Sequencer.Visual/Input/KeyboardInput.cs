using System;
using Avalonia.Input;

namespace Sequencer.Visual.Input
{
    /// <summary>
    /// A key with the exact modifiers held, used to look up keyboard shortcuts.
    /// </summary>
    public sealed class KeyboardInput : IEquatable<KeyboardInput>, IKeyboardInput
    {
        public KeyboardInput(KeyModifiers modifiers, Key keyPress)
        {
            Modifiers = modifiers;
            KeyPress = keyPress;
        }

        public KeyboardInput(Key keyPress) : this(KeyModifiers.None, keyPress)
        {
        }

        public KeyModifiers Modifiers { get; }

        public Key KeyPress { get; }

        public bool Equals(KeyboardInput other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return ReferenceEquals(this, other) || (Modifiers == other.Modifiers && KeyPress == other.KeyPress);
        }

        public override bool Equals(object obj)
        {
            return obj is KeyboardInput other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Modifiers, KeyPress);
        }
    }
}
