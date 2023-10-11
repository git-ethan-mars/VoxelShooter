using System;
using UnityEngine;

namespace Data
{
    public readonly struct BlockData : IEquatable<BlockData>
    {
        public readonly Color32 Color;

        public BlockData(Color32 color)
        {
            Color = color;
        }

        public bool Equals(BlockData other)
        {
            return Color.Equals(other.Color);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Color.GetHashCode();
        }
        
        public bool IsSolid()
        {
            return !(Color.a == BlockColor.empty.a &&
                     Color.r == BlockColor.empty.r &&
                     Color.g == BlockColor.empty.g &&
                     Color.b == BlockColor.empty.b);
        }
    }
}