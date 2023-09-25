using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Data
{
    public class MapSceneData
    {
        public readonly LightData LightData;
        public readonly Material SkyboxMaterial;
        public readonly AmbientMode AmbientMode;
        public readonly Color SkyColor;
        public readonly Color EquatorColor;
        public readonly Color GroundColor;
        public readonly float AmbientIntensity;
        public readonly bool IsFogActivated;
        public readonly FogMode FogMode;
        public readonly Color FogColor;
        public readonly float FogStartDistance;
        public readonly float FogEndDistance;
        public readonly float FogDensity;
        public readonly List<SpawnPointData> SpawnPoints;

        public MapSceneData(MapConfigure configure)
        {
            LightData = configure.lightData;
            SkyboxMaterial = configure.skyboxMaterial;
            AmbientMode = configure.ambientMode;
            SkyColor = configure.skyColor;
            EquatorColor = configure.equatorColor;
            GroundColor = configure.groundColor;
            AmbientIntensity = configure.ambientIntensity;
            IsFogActivated = configure.isFogActivated;
            FogMode = configure.fogMode;
            FogColor = configure.fogColor;
            FogStartDistance = configure.fogStartDistance;
            FogEndDistance = configure.fogEndDistance;
            FogDensity = configure.fogDensity;
            SpawnPoints = new List<SpawnPointData>(configure.spawnPoints);
        }
    }
}