﻿using System.Windows.Input;

namespace Sequencer.Input
{
    public class KeyboardStateProcessor : IKeyboardStateProcessor
    {
        public bool IsKeyDown(Key key)
        {
            return Keyboard.IsKeyDown(key);
        }
    }
}