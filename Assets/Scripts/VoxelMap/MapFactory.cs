using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VoxelMap.Data;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;
namespace VoxelMap
{
	public class MapFactory : IMapFactory
	{
		private const string WallContainerName = "Walls";
		private const string SpawnPointContainerName = "Spawnpoints";

		private const string DirectionalLightName = "Directional Light";
		private const string SpawnPointPath = "Prefabs/MapCreation/Spawnpoint";
		private const string ChunkMeshRendererPath = "Prefabs/MapCreation/Chunk";
		private const string WallPath = "Prefabs/MapCreation/Wall";
		private const string MapProviderPath = "Prefabs/MapCreation/VoxelMap";

		public GameObject CreateChunkView(Vector3 position, Transform parent)
		{
			GameObject chunkView = Object.Instantiate(Resources.Load<GameObject>(ChunkMeshRendererPath), position, Quaternion.identity, parent);
			return chunkView;
		}

		public void CreateWalls(MapData mapData, Transform parent)
		{
			Transform wallContainer = new GameObject(WallContainerName).transform;
			wallContainer.SetParent(parent);
			var allFaces = Enum.GetValues(typeof(Face)).Cast<Face>().Where(face => face != Face.None);
			foreach (Face face in allFaces)
			{
				GameObject wall = Object.Instantiate(Resources.Load<GameObject>(WallPath), wallContainer);
				var mesh = new Mesh();
				mesh.vertices = new Vector3[4];
				if (face == Face.Top)
				{
					var startPoint = new Vector3(0, mapData.Height, 0);
					var endPoint = new Vector3(mapData.Width, mapData.Height,
						mapData.Depth);
					var secondPoint = new Vector3(0, mapData.Height, mapData.Depth);
					var thirdPoint = new Vector3(mapData.Width, mapData.Height, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
				}

				if (face == Face.Bottom)
				{
					var startPoint = new Vector3(0, 0, 0);
					var endPoint = new Vector3(mapData.Width, 0, mapData.Depth);
					var secondPoint = new Vector3(0, 0, mapData.Depth);
					var thirdPoint = new Vector3(mapData.Width, 0, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 1, 2, 3, 2, 1 };
				}

				if (face == Face.Left)
				{
					var startPoint = new Vector3(0, 0, 0);
					var endPoint = new Vector3(0, mapData.Height, mapData.Depth);
					var secondPoint = new Vector3(0, 0, mapData.Depth);
					var thirdPoint = new Vector3(0, mapData.Height, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
				}

				if (face == Face.Right)
				{
					var startPoint = new Vector3(mapData.Width, 0, 0);
					var endPoint = new Vector3(mapData.Width, mapData.Height,
						mapData.Depth);
					var secondPoint = new Vector3(mapData.Width, 0, mapData.Depth);
					var thirdPoint = new Vector3(mapData.Width, mapData.Height, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 1, 2, 3, 2, 1 };
				}

				if (face == Face.Front)
				{
					var startPoint = new Vector3(0, 0, mapData.Depth);
					var endPoint = new Vector3(mapData.Width, mapData.Height,
						mapData.Depth);
					var secondPoint = new Vector3(mapData.Width, 0, mapData.Depth);
					var thirdPoint = new Vector3(0, mapData.Height, mapData.Depth);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
				}

				if (face == Face.Back)
				{
					var startPoint = new Vector3(0, 0, 0);
					var endPoint = new Vector3(mapData.Width, mapData.Height, 0);
					var secondPoint = new Vector3(mapData.Width, 0, 0);
					var thirdPoint = new Vector3(0, mapData.Height, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 1, 2, 3, 2, 1 };
				}

				wall.GetComponent<MeshFilter>().mesh = mesh;
				wall.GetComponent<MeshCollider>().sharedMesh = mesh;
			}
		}

		public void CreateDirectionalLight(DirectionalLightData directionalLightData, Transform parent)
		{
			var light = new GameObject(DirectionalLightName).AddComponent<Light>();
			light.transform.position = directionalLightData.position;
			light.transform.rotation = directionalLightData.rotation;
			light.color = directionalLightData.color;
			light.type = LightType.Directional;
			light.shadows = LightShadows.Soft;
			light.shadowBias = directionalLightData.bias;
			light.shadowNormalBias = directionalLightData.normalBias;
			light.transform.SetParent(parent);
		}

		public void CreateSpawnPoints(List<SpawnPointData> data, Transform parent)
		{
			Transform spawnPointContainer = new GameObject(SpawnPointContainerName).transform;
			spawnPointContainer.SetParent(parent);

			for (var i = 0; i < data.Count; i++)
			{
				Object.Instantiate(Resources.Load<GameObject>(SpawnPointPath), data[i].position,
					Quaternion.identity, spawnPointContainer);
			}
		}

		public Map CreateEmptyMap()
		{
			return Object.Instantiate(Resources.Load<GameObject>(MapProviderPath)).GetComponent<Map>();
		}
	}
}