using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public struct BlockData : IEquatable<BlockData>
    {
        public Color32 color;

        public BlockData(Color32 color)
        {
            this.color = color;
        }

        public bool Equals(BlockData other)
        {
            return color.Equals(other.color);
        }

        public override bool Equals(object obj)
        {
            return obj is BlockData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return color.GetHashCode();
        }
        
        public bool IsSolid()
        {
            return !(color.a == BlockColor.empty.a &&
                     color.r == BlockColor.empty.r &&
                     color.g == BlockColor.empty.g &&
                     color.b == BlockColor.empty.b);
        }
    }
}