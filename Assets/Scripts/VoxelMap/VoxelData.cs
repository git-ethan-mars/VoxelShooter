using System;
using UnityEngine;
namespace VoxelMap
{
	public readonly struct VoxelData : IEquatable<VoxelData>
	{
		public static readonly VoxelData Air = new VoxelData(new Color32(0, 0, 0, 0));
		public static readonly VoxelData DefaultInner = new VoxelData(new Color32(1, 1, 1, 0));

		public readonly Color32 Color;

		public VoxelData(Color32 color)
		{
			Color = color;
		}

		public bool Equals(VoxelData other)
		{
			return Color.r == other.Color.r && Color.g == other.Color.g && Color.b == other.Color.b &&
			       Color.a == other.Color.a;
		}

		public override bool Equals(object obj)
		{
			return obj is VoxelData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Color.GetHashCode();
		}

		public bool IsSolid()
		{
			return !Equals(Air);
		}

		public static bool operator ==(VoxelData first, VoxelData second)
		{
			return first.Equals(second);
		}

		public static bool operator !=(VoxelData first, VoxelData second)
		{
			return !(first == second);
		}
	}
}