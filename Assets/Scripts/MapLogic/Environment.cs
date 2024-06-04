using Data;
using UnityEngine;

namespace MapLogic
{
    public static class Environment
    {
        public static void ApplyAmbientLighting(MapConfigure mapConfigure)
        {
            RenderSettings.skybox = mapConfigure.skyboxMaterial;
            RenderSettings.ambientMode = mapConfigure.ambientMode;
            RenderSettings.ambientSkyColor = mapConfigure.skyColor;
            RenderSettings.ambientEquatorColor = mapConfigure.equatorColor;
            RenderSettings.ambientGroundColor = mapConfigure.groundColor;
            RenderSettings.ambientIntensity = mapConfigure.ambientIntensity;
            DynamicGI.UpdateEnvironment();
        }

        public static void ApplyFog(MapConfigure mapConfigure)
        {
            RenderSettings.fog = mapConfigure.isFogActivated;
            RenderSettings.fogMode = mapConfigure.fogMode;
            RenderSettings.fogColor = mapConfigure.fogColor;
            RenderSettings.fogStartDistance = mapConfigure.fogStartDistance;
            RenderSettings.fogEndDistance = mapConfigure.fogEndDistance;
            RenderSettings.fogDensity = mapConfigure.fogDensity;
        }
    }
}