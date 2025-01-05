using System;

namespace VoxelMap
{
    [Flags]
    public enum Face : byte
    {
        None = 0,
        Top = 1,
        Bottom = 2,
        Front = 4,
        Back = 8,
        Right = 16,
        Left = 32
    }
}