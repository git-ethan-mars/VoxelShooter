using System;
using UnityEngine;
using UnityEngine.Rendering;
using VoxelMap.CustomAttributes;

namespace VoxelMap
{
	[Serializable]
	public class AmbientData
	{
		[ReadOnly]
		public AmbientMode mode;

		[ReadOnly]
		public Color skyColor;

		[ReadOnly]
		public Color equatorColor;

		[ReadOnly]
		public Color groundColor;

		[ReadOnly]
		public float intensity;

		public AmbientData(AmbientMode mode, Color skyColor, Color equatorColor, Color groundColor, float intensity)
		{
			this.mode = mode;
			this.skyColor = skyColor;
			this.equatorColor = equatorColor;
			this.groundColor = groundColor;
			this.intensity = intensity;
		}
	}
}