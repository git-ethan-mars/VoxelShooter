using System.Collections.Generic;
using Data;
using UnityEngine;
namespace VoxelMap
{
	public interface IMapFactory
	{
		GameObject CreateChunkView(Vector3 position, Transform parent);
		void CreateWalls(MapData mapData, Transform parent);
		void CreateWaterPlane(Vector3 position, Color32 waterColor, Transform parent);
		void CreateDirectionalLight(LightData lightData, Transform parent);
		void CreateSpawnPoints(List<SpawnPointData> data, Transform parent);
		Map CreateEmptyMap();
	}
}