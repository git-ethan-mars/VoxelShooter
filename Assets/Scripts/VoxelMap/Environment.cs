using UnityEngine;

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

        public static void ApplyFog(FogData fogData)
        {
            RenderSettings.fog = fogData.activated;
            RenderSettings.fogMode = fogData.mode;
            RenderSettings.fogColor = fogData.color;
            RenderSettings.fogStartDistance = fogData.startDistance;
            RenderSettings.fogEndDistance = fogData.endDistance;
            RenderSettings.fogDensity = fogData.density;
        }
    }
}