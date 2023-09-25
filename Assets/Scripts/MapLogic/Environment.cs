using Data;
using UnityEngine;

namespace MapLogic
{
    public static class Environment
    {
        public static void ApplyAmbientLighting(MapSceneData sceneData)
        {
            RenderSettings.skybox = sceneData.SkyboxMaterial;
            RenderSettings.ambientMode = sceneData.AmbientMode;
            RenderSettings.ambientSkyColor = sceneData.SkyColor;
            RenderSettings.ambientEquatorColor = sceneData.EquatorColor;
            RenderSettings.ambientGroundColor = sceneData.GroundColor;
            RenderSettings.ambientIntensity = sceneData.AmbientIntensity;
        }

        public static void ApplyFog(MapSceneData sceneData)
        {
            RenderSettings.fog = sceneData.IsFogActivated;
            RenderSettings.fogMode = sceneData.FogMode;
            RenderSettings.fogColor = sceneData.FogColor;
            RenderSettings.fogStartDistance = sceneData.FogStartDistance;
            RenderSettings.fogEndDistance = sceneData.FogEndDistance;
            RenderSettings.fogDensity = sceneData.FogDensity;
        }
    }
}