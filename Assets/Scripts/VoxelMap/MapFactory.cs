using System;
using System.Collections.Generic;
using System.Linq;
using Common.AssetManagement;
using Common.StaticData;
using UnityEngine;

namespace VoxelMap
{
	public class MapFactory : IMapFactory
	{
		private const string DirectionalLightName = "Directional Light";
		private const string WallContainerName = "WallContainer";
		private const string SpawnPointPath = "Prefabs/MapCreation/Spawnpoint";
		private const string MapPath = "Prefabs/MapCreation/Map";
		private const float WaterScale = 1024;

		private readonly IAssetProvider _assets;

		public MapFactory(IAssetProvider assets)
		{
			_assets = assets;
		}

		public GameObject CreateChunk(Vector3 position, Quaternion rotation, Transform parent)
		{
			return _assets.Instantiate(MeshPath.ChunkMeshRendererPath, position, rotation, parent);
		}

		public GameObject[] CreateWalls(MapData mapData, Transform parent)
		{
			var walls = new GameObject[6];
			var allFaces = Enum.GetValues(typeof(Faces)).Cast<Faces>().Where(face => face != Faces.None);
			foreach (var face in allFaces)
			{
				var wall = _assets.Instantiate(MeshPath.WallPath, parent);
				var mesh = new Mesh();
				mesh.vertices = new Vector3[4];
				if (face == Faces.Top)
				{
					var startPoint = new Vector3(0, mapData.Height, 0);
					var endPoint = new Vector3(mapData.Width, mapData.Height,
						mapData.Depth);
					var secondPoint = new Vector3(0, mapData.Height, mapData.Depth);
					var thirdPoint = new Vector3(mapData.Width, mapData.Height, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
				}

				if (face == Faces.Bottom)
				{
					var startPoint = new Vector3(0, 0, 0);
					var endPoint = new Vector3(mapData.Width, 0, mapData.Depth);
					var secondPoint = new Vector3(0, 0, mapData.Depth);
					var thirdPoint = new Vector3(mapData.Width, 0, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 1, 2, 3, 2, 1 };
				}

				if (face == Faces.Left)
				{
					var startPoint = new Vector3(0, 0, 0);
					var endPoint = new Vector3(0, mapData.Height, mapData.Depth);
					var secondPoint = new Vector3(0, 0, mapData.Depth);
					var thirdPoint = new Vector3(0, mapData.Height, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
				}

				if (face == Faces.Right)
				{
					var startPoint = new Vector3(mapData.Width, 0, 0);
					var endPoint = new Vector3(mapData.Width, mapData.Height,
						mapData.Depth);
					var secondPoint = new Vector3(mapData.Width, 0, mapData.Depth);
					var thirdPoint = new Vector3(mapData.Width, mapData.Height, 0);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 1, 2, 3, 2, 1 };
				}

				if (face == Faces.Front)
				{
					var startPoint = new Vector3(0, 0, mapData.Depth);
					var endPoint = new Vector3(mapData.Width, mapData.Height,
						mapData.Depth);
					var secondPoint = new Vector3(mapData.Width, 0, mapData.Depth);
					var thirdPoint = new Vector3(0, mapData.Height, mapData.Depth);
					mesh.vertices = new[] { startPoint, secondPoint, thirdPoint, endPoint };
					mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
				}

				if (face == Faces.Back)
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

			return walls;
		}

		public GameObject CreateWaterPlane(Vector3 position, Color32 waterColor, Transform parent)
		{
			var waterPlane = _assets.Instantiate(MeshPath.WaterPlane, position, Quaternion.identity);
			waterPlane.transform.SetParent(parent);
			waterPlane.transform.localScale *= WaterScale;
			var mesh = new Mesh();
			mesh.SetVertices(new List<Vector3>()
			{ new(-0.5f, 0, -0.5f), new(-0.5f, 0, 0.5f), new(0.5f, 0, -0.5f), new(0.5f, 0, 0.5f) });
			mesh.SetTriangles(new[] { 0, 1, 2, 1, 3, 2 }, 0);
			mesh.SetColors(Enumerable.Range(0, mesh.vertexCount).Select(_ => waterColor).ToList());
			mesh.SetNormals(Enumerable.Range(0, mesh.vertexCount).Select(_ => Vector3.up).ToList());
			waterPlane.GetComponent<MeshFilter>().sharedMesh = mesh;
			return waterPlane;
		}

		public Light CreateDirectionalLight(LightData lightData, Transform parent)
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
			return light;
		}

		public GameObject[] CreateSpawnPoints(List<SpawnPointData> data, Transform container)
		{
			var spawnPoints = new GameObject[data.Count];
			for (var i = 0; i < data.Count; i++)
			{
				var spawnPoint = _assets.Instantiate(SpawnPointPath, data[i].position,
					Quaternion.identity, container);
				spawnPoints[i] = spawnPoint;
			}

			return spawnPoints;
		}

		public Map CreateMap(MapData mapData, Transform container)
		{
			var mapObject = _assets.Instantiate(MapPath);
			mapObject.transform.SetParent(container);
			var map = mapObject.GetComponent<Map>();
			map.Construct(mapData);
			return map;
		}
	}
}