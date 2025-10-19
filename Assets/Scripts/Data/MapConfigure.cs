using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace Data
{
	[CreateAssetMenu]
	public class MapConfigure : ScriptableObject
	{
		[field: Header("Image")]
		[field: SerializeField]
		public Sprite Image { get; private set; }

		[field: Header("Color")]
		[field: SerializeField]
		public Color32 WaterColor { get; private set; }

		[field: SerializeField]
		public Color32 InnerColor { get; private set; }

		[field: Header("Lighting")]
		[field: SerializeField]
		public LightData LightData { get; private set; } = new LightData(Vector3.zero, Quaternion.identity, Color.white, 0, 0);

		[field: Header("Skybox")]
		[field: SerializeField]
		public Material SkyboxMaterial { get; private set; }

		[field: Header("Ambient light")]
		[field: SerializeField]
		public AmbientData AmbientData { get; private set; } = new AmbientData(AmbientMode.Skybox, new Color32(54, 58, 66, 255),
			new Color32(29, 32, 34, 255), new Color32(12, 11, 9, 255), 1);

		[field: Header("Fog")]
		[field: SerializeField]
		public FogData FogData { get; private set; }

		[field: Header("Weather")]
		[field: SerializeField]
		public ParticleSystem Weather { get; private set; }

		[field: Header("Spawn points")]
		[field: SerializeField]
		public List<SpawnPointData> SpawnPoints { get; set; } = new List<SpawnPointData>();

	}
}