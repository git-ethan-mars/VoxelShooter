using System;
using UnityEngine;
namespace Data
{
	[Serializable]
	public struct LightData : IEquatable<LightData>
	{
		public Vector3 position;
		public Quaternion rotation;
		public Color color;
		public float bias;
		public float normalBias;

		public LightData(Vector3 position, Quaternion rotation, Color color, float bias, float normalBias)
		{
			this.position = position;
			this.rotation = rotation;
			this.color = color;
			this.bias = bias;
			this.normalBias = normalBias;
		}

		public bool Equals(LightData other)
		{
			return position.Equals(other.position) && rotation.Equals(other.rotation) && 
			       color.Equals(other.color) && bias.Equals(other.bias) && normalBias.Equals(other.normalBias);
		}

		public override bool Equals(object obj)
		{
			return obj is LightData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(position, rotation, color, bias, normalBias);
		}
	}
}