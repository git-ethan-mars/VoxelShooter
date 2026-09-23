using System;
using UnityEngine;
using UnityEngine.Rendering;
namespace VoxelMap.Data
{
	[Serializable]
	public struct AmbientData : IEquatable<AmbientData>
	{
		public AmbientMode mode;
		public Color skyColor;
		public Color equatorColor;
		public Color groundColor;
		public float intensity;

		public AmbientData(AmbientMode mode, Color skyColor, Color equatorColor, Color groundColor, float intensity)
		{
			this.mode = mode;
			this.skyColor = skyColor;
			this.equatorColor = equatorColor;
			this.groundColor = groundColor;
			this.intensity = intensity;
		}

		public bool Equals(AmbientData other)
		{
			return mode == other.mode && skyColor.Equals(other.skyColor) && equatorColor.Equals(other.equatorColor) 
			       && groundColor.Equals(other.groundColor) && intensity.Equals(other.intensity);
		}

		public override bool Equals(object obj)
		{
			return obj is AmbientData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)mode, skyColor, equatorColor, groundColor, intensity);
		}
	}
}