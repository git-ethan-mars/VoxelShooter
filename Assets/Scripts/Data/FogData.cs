using System;
using UnityEngine;
namespace Data
{
	[Serializable]
	public struct FogData : IEquatable<FogData>
	{
		public bool activated;
		public FogMode mode;
		public Color color;
		public float startDistance;
		public float endDistance;
		public float density;

		public FogData(bool activated, FogMode mode, Color color, float startDistance, float endDistance, float density)
		{
			this.activated = activated;
			this.mode = mode;
			this.color = color;
			this.startDistance = startDistance;
			this.endDistance = endDistance;
			this.density = density;
		}

		public bool Equals(FogData other)
		{
			return activated == other.activated && mode == other.mode && color.Equals(other.color) && 
			       startDistance.Equals(other.startDistance) && endDistance.Equals(other.endDistance) &&
			       density.Equals(other.density);
		}

		public override bool Equals(object obj)
		{
			return obj is FogData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(activated, (int)mode, color, startDistance, endDistance, density);
		}
	}
}