using System;
using UnityEngine;

namespace VoxelMap
{
    public readonly struct BlockData : IEquatable<BlockData>
    {
        public static BlockData Air = new(new Color32(0,0,0,0));
        public static BlockData Inner = new(new Color32(1, 1, 1, 0));
        
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
            return !Equals(Air);
        }

        public static bool operator ==(BlockData first, BlockData second)
        {
            return first.Equals(second);
        }

        public static bool operator !=(BlockData first, BlockData second)
        {
            return !(first == second);
        }
    }
}