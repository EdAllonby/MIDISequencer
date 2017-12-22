﻿using System;

namespace Sequencer.View.Console
{
    [Flags]
    internal enum DesiredAccess : uint
    {
        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        GenericExecute = 0x20000000,
        GenericAll = 0x10000000
    }
}