using System;
using Common.CustomAttributes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Common.StaticData
{
	[Serializable]
	public class AmbientData
	{
		[FormerlySerializedAs("AmbientMode")]
		[ReadOnly]
		public AmbientMode mode;

		[FormerlySerializedAs("SkyColor")]
		[ReadOnly]
		public Color skyColor;

		[FormerlySerializedAs("EquatorColor")]
		[ReadOnly]
		public Color equatorColor;

		[FormerlySerializedAs("GroundColor")]
		[ReadOnly]
		public Color groundColor;

		[FormerlySerializedAs("AmbientIntensity")]
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