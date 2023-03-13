using System;

namespace Core
{
    [Flags]
    public enum Faces : byte
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Front = 4,
        Back = 8,
        Left = 16,
        Right = 32
        
    }
}