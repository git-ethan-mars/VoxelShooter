using System.Collections.Generic;
using Common.CustomAttributes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Common.StaticData
{
	[CreateAssetMenu]
	public class MapConfigure : ScriptableObject
	{
		public Texture2D Image => image;

		[Header("Image")]
		[ReadOnly]
		[SerializeField]
		private Texture2D image;

		public Color32 WaterColor => waterColor;

		[Header("Color")]
		[ReadOnly]
		[SerializeField]
		private Color32 waterColor = new(3, 58, 135, 255);

		public Color32 InnerColor => innerColor;

		[ReadOnly]
		[SerializeField]
		private Color32 innerColor = new(89, 53, 47, 255);

		public LightData LightData => lightData;

		[Header("Lighting")]
		[SerializeField]
		private LightData lightData = new(Vector3.zero, Quaternion.identity, Color.white, 0, 0);

		public Material SkyboxMaterial => skyboxMaterial;

		[Header("Skybox")]
		[ReadOnly]
		[SerializeField]
		private Material skyboxMaterial;

		public AmbientData AmbientData => ambientData;

		[Header("Ambient light")]
		[SerializeField]
		private AmbientData ambientData = new(AmbientMode.Skybox, new Color32(54, 58, 66, 255), new Color32(29, 32, 34, 255),
			new Color32(12, 11, 9, 255), 1);

		public FogData FogData => fogData;

		[Header("Fog")]
		[SerializeField]
		private FogData fogData = new(false, FogMode.Linear, new Color(0.5f, 0.5f, 0.5f, 1), 0, 300, 0.01f);

		[Header("Spawn points")]
		[SerializeField]
		public List<SpawnPointData> spawnPoints = new();

		public List<Vector3> SPAWNPOINTS = new();

		public ParticleSystem Weather => weather;

		[Header("Weather")]
		[ReadOnly]
		[SerializeField]
		private ParticleSystem weather;
	}
}