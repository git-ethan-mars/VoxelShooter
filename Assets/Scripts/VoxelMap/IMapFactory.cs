using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace VoxelMap
{
	public interface IMapFactory
	{
		Chunk CreateChunk(Vector3 position, Transform parent, ChunkData chunkData, NativeArray<Face> faces);
		GameObject[] CreateWalls(MapData mapData, Transform parent);
		GameObject CreateWaterPlane(Vector3 position, Color32 waterColor, Transform parent);
		Light CreateDirectionalLight(LightData lightData, Transform parent);
		Map CreateMap(MapData mapData, Transform container);
		GameObject[] CreateSpawnPoints(List<SpawnPointData> data, Transform container);
	}
}