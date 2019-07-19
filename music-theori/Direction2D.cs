﻿using System;

namespace theori
{
    [Flags]
    public enum Direction2D
    {
        None = 0,

        Left  = 0b0001,
        Right = 0b0011,

        Upd   = 0b0100,
        Down  = 0b1100,
    }
}
