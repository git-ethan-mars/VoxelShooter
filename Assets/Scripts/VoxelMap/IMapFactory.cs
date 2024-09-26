using System.Collections.Generic;
using Common.StaticData;
using UnityEngine;

namespace VoxelMap
{
	public interface IMapFactory
	{
		GameObject CreateChunk(Vector3 position, Quaternion rotation, Transform parent);
		GameObject[] CreateWalls(MapData mapData, Transform parent);
		GameObject CreateWaterPlane(Vector3 position, Color32 waterColor, Transform parent);
		Light CreateDirectionalLight(LightData lightData, Transform parent);
		Map CreateMap(MapData mapData, Transform container);
		GameObject[] CreateSpawnPoints(List<SpawnPointData> data, Transform container);
	}
}