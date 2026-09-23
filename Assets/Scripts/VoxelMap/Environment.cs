using UnityEngine;
using VoxelMap.Data;

namespace VoxelMap
{
	public static class Environment
	{
		public static void ApplySkybox(Material material)
		{
			RenderSettings.skybox = material;
			DynamicGI.UpdateEnvironment();
		}

		public static void ApplyAmbientLighting(AmbientData ambientData)
		{
			RenderSettings.ambientMode = ambientData.mode;
			RenderSettings.ambientSkyColor = ambientData.skyColor;
			RenderSettings.ambientEquatorColor = ambientData.equatorColor;
			RenderSettings.ambientGroundColor = ambientData.groundColor;
			RenderSettings.ambientIntensity = ambientData.intensity;
		}
	}
}
