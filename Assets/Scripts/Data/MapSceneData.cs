using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    public class MapSceneData
    {
        public readonly LightData LightData;
        public readonly Material Skybox;
        public readonly List<SpawnPointData> SpawnPoints;

        public MapSceneData(MapConfigure configure)
        {
            LightData = configure.lightData;
            Skybox = configure.skyboxMaterial;
            SpawnPoints = new List<SpawnPointData>(configure.spawnPoints);
        }
    }
}