using System.Collections.Generic;
using UnityEngine;
using VoxelMap.Data;
namespace VoxelMap
{
	public interface IMapFactory
	{
		GameObject CreateChunkView(Vector3 position, Transform parent);
		void CreateWalls(MapData mapData, Transform parent);
		void CreateDirectionalLight(DirectionalLightData directionalLightData, Transform parent);
		
		void CreateSpawnPoints(List<SpawnPointData> data, Transform parent);
		Map CreateEmptyMap();
	}
}