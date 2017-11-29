using System;
using System.Windows.Input;

namespace Sequencer.Input
{
    public sealed class KeyboardInput : IEquatable<KeyboardInput>, IKeyboardInput
    {
        public KeyboardInput(ModifierKeys modifiers, Key keyPress)
        {
            Modifiers = modifiers;
            KeyPress = keyPress;
        }

        public KeyboardInput(Key keyPress)
        {
            Modifiers = ModifierKeys.None;
            KeyPress = keyPress;
        }

        public ModifierKeys Modifiers { get; }

        public Key KeyPress { get; }

        public bool Equals(KeyboardInput other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var isit =  Modifiers == other.Modifiers && KeyPress == other.KeyPress;
            return isit;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var a = obj as KeyboardInput;
            return a != null && Equals(a);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Modifiers * 397) ^ (int) KeyPress;
            }
        }
    }
}