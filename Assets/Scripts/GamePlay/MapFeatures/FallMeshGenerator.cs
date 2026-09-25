using System.Collections.Generic;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public class FallMeshGenerator
	{
		// The chunk shader reads ambient occlusion from vertex alpha; 3 means unoccluded.
		private const byte UnoccludedAlpha = 3;

		private readonly FallingMeshParticlePool _fallingMeshParticlePool;
		private readonly IMeshFactory _meshFactory;

		public FallMeshGenerator(IParticleFactory particleFactory, IMeshFactory meshFactory)
		{
			_fallingMeshParticlePool = new FallingMeshParticlePool(particleFactory);
			_meshFactory = meshFactory;
		}

		public void GenerateFallVoxels(IReadOnlyList<Voxel> voxels)
		{
			if (voxels.Count == 0)
			{
				return;
			}

			var occupied = new HashSet<Vector3Int>();
			Vector3 center = Vector3.zero;

			foreach (Voxel voxel in voxels)
			{
				var position = new Vector3Int(voxel.Position.x, voxel.Position.y, voxel.Position.z);
				occupied.Add(position);
				center += position;
			}

			// The mesh is built around the component centre, so it tumbles around its middle.
			center = center / voxels.Count + Vector3.one * 0.5f;
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var colors = new List<Color32>();
			var triangles = new List<int>();
			var uvs = new List<Vector2>();

			foreach (Voxel voxel in voxels)
			{
				var position = new Vector3Int(voxel.Position.x, voxel.Position.y, voxel.Position.z);
				Vector3 origin = position - center;
				Color32 color = voxel.Data.Color;
				color.a = UnoccludedAlpha;

				// Corners of every face go clockwise when seen from outside.
				if (!occupied.Contains(position + Vector3Int.up))
				{
					AddFace(origin, new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0), Vector3.up, color,
						vertices, normals, colors, uvs, triangles);
				}

				if (!occupied.Contains(position + Vector3Int.down))
				{
					AddFace(origin, new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), Vector3.down, color,
						vertices, normals, colors, uvs, triangles);
				}

				if (!occupied.Contains(position + Vector3Int.right))
				{
					AddFace(origin, new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1), Vector3.right, color,
						vertices, normals, colors, uvs, triangles);
				}

				if (!occupied.Contains(position + Vector3Int.left))
				{
					AddFace(origin, new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0), Vector3.left, color,
						vertices, normals, colors, uvs, triangles);
				}

				if (!occupied.Contains(position + Vector3Int.forward))
				{
					AddFace(origin, new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 1), Vector3.forward, color,
						vertices, normals, colors, uvs, triangles);
				}

				if (!occupied.Contains(position + Vector3Int.back))
				{
					AddFace(origin, new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0), Vector3.back, color,
						vertices, normals, colors, uvs, triangles);
				}
			}

			var meshData = new MeshData(vertices, triangles, colors, normals, uvs);
			_meshFactory.CreateFallingMesh(meshData, _fallingMeshParticlePool, center);
		}

		private static void AddFace(Vector3 origin, Vector3 first, Vector3 second, Vector3 third, Vector3 fourth, Vector3 normal, Color32 color,
			List<Vector3> vertices, List<Vector3> normals, List<Color32> colors, List<Vector2> uvs, List<int> triangles)
		{
			int startIndex = vertices.Count;
			vertices.Add(origin + first);
			vertices.Add(origin + second);
			vertices.Add(origin + third);
			vertices.Add(origin + fourth);

			for (int i = 0; i < 4; i++)
			{
				normals.Add(normal);
				colors.Add(color);
			}

			uvs.Add(new Vector2(0, 0));
			uvs.Add(new Vector2(0, 1));
			uvs.Add(new Vector2(1, 1));
			uvs.Add(new Vector2(1, 0));

			triangles.Add(startIndex);
			triangles.Add(startIndex + 1);
			triangles.Add(startIndex + 2);
			triangles.Add(startIndex);
			triangles.Add(startIndex + 2);
			triangles.Add(startIndex + 3);
		}
	}
}
