using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Data;
using Services;
using UnityEngine;
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
		private const string WaterPlane = "Prefabs/MapCreation/WaterPlane";
		private const string ChunkMeshRendererPath = "Prefabs/MapCreation/Chunk";
		private const string WallPath = "Prefabs/MapCreation/Wall";
		private const string MapProviderPath = "Prefabs/MapCreation/VoxelMap";
		private const float WaterScale = 1024;

		private readonly IAssetProvider _assets;

		public MapFactory(IAssetProvider assets)
		{
			_assets = assets;
		}

		public GameObject CreateChunkView(Vector3 position, Transform parent)
		{
			GameObject chunkView = _assets.Instantiate(ChunkMeshRendererPath, position, Quaternion.identity, parent);
			return chunkView;
		}

		public void CreateWalls(MapData mapData, Transform parent)
		{
			Transform wallContainer = new GameObject(WallContainerName).transform;
			wallContainer.SetParent(parent);
			var allFaces = Enum.GetValues(typeof(Face)).Cast<Face>().Where(face => face != Face.None);
			foreach (Face face in allFaces)
			{
				GameObject wall = _assets.Instantiate(WallPath, wallContainer);
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

		public void CreateWaterPlane(Vector3 position, Color32 waterColor, Transform parent)
		{
			GameObject waterPlane = _assets.Instantiate(WaterPlane, position - Vector3.up * 0.001f, Quaternion.identity);
			waterPlane.transform.SetParent(parent);
			waterPlane.transform.localScale *= WaterScale;
			var mesh = new Mesh();
			mesh.SetVertices(new List<Vector3>
				{ new Vector3(-0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(0.5f, 0, 0.5f) });
			mesh.SetTriangles(new[] { 0, 1, 2, 1, 3, 2 }, 0);
			mesh.SetNormals(Enumerable.Range(0, mesh.vertexCount).Select(_ => Vector3.up).ToList());
			waterPlane.GetComponent<MeshFilter>().sharedMesh = mesh;
			var meshRenderer = waterPlane.GetComponent<MeshRenderer>();
			var material = new Material(meshRenderer.sharedMaterial);
			material.color = waterColor;
			meshRenderer.sharedMaterial = material;
		}

		public void CreateDirectionalLight(LightData lightData, Transform parent)
		{
			var light = new GameObject(DirectionalLightName).AddComponent<Light>();
			light.transform.position = lightData.position;
			light.transform.rotation = lightData.rotation;
			light.color = lightData.color;
			light.type = LightType.Directional;
			light.shadows = LightShadows.Soft;
			light.shadowBias = lightData.bias;
			light.shadowNormalBias = lightData.normalBias;
			light.transform.SetParent(parent);
		}

		public void CreateSpawnPoints(List<SpawnPointData> data, Transform parent)
		{
			Transform spawnPointContainer = new GameObject(SpawnPointContainerName).transform;
			spawnPointContainer.SetParent(parent);

			for (var i = 0; i < data.Count; i++)
			{
				_assets.Instantiate(SpawnPointPath, data[i].position,
					Quaternion.identity, spawnPointContainer);
			}
		}

		public Map CreateEmptyMap()
		{
			return _assets.Instantiate(MapProviderPath).GetComponent<Map>();
		}
	}
}