using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace VoxelMap
{
	public class MapGenerator
	{
		private readonly MapData _mapData;
		private readonly IMapFactory _mapFactory;

		public MapGenerator(IMapFactory mapFactory, MapData mapData)
		{
			_mapData = mapData;
			_mapFactory = mapFactory;
		}

		public unsafe Chunk[] GenerateChunks(Transform parent)
		{
			var handles = new ulong[_mapData.ChunkCount];
			var blockAddresses = new void*[_mapData.ChunkCount];

			for (var i = 0; i < blockAddresses.Length; i++)
			{
				blockAddresses[i] =
					UnsafeUtility.PinGCArrayAndGetDataAddress(_mapData.GetChunkDataByIndex(i).Blocks, out handles[i]);
			}

			var jobs = InitializeJobs(blockAddresses);
			using var jobHandles = RunJobs(jobs);
			var meshes = CreateChunkMeshes(jobs, parent);

			for (var i = 0; i < handles.Length; i++)
			{
				UnsafeUtility.ReleaseGCObject(handles[i]);
			}

			DisposeJobs(jobs);
			return meshes;
		}

		private void SetChunkNeighbours(IReadOnlyList<Chunk> meshes)
		{
			for (var i = 0; i < meshes.Count; i++)
			{
				if (TryGetUpChunkNumber(i, out var upChunkNumber))
				{
					meshes[i].SetNeighbour(ChunkNeighbourType.Up, meshes[upChunkNumber]);
				}

				if (TryGetDownChunkNumber(i, out var downChunkNumber))
				{
					meshes[i].SetNeighbour(ChunkNeighbourType.Down, meshes[downChunkNumber]);
				}

				if (TryGetFrontChunkNumber(i, out var frontChunkNumber))
				{
					meshes[i].SetNeighbour(ChunkNeighbourType.Front, meshes[frontChunkNumber]);
				}

				if (TryGetBackChunkNumber(i, out var backChunkNumber))
				{
					meshes[i].SetNeighbour(ChunkNeighbourType.Back, meshes[backChunkNumber]);
				}

				if (TryGetRightChunkNumber(i, out var rightChunkNumber))
				{
					meshes[i].SetNeighbour(ChunkNeighbourType.Right, meshes[rightChunkNumber]);
				}

				if (TryGetLeftChunkNumber(i, out var leftChunkNumber))
				{
					meshes[i].SetNeighbour(ChunkNeighbourType.Left, meshes[leftChunkNumber]);
				}
			}
		}

		private unsafe ChunkMeshGenerator[] InitializeJobs(void*[] blockAddresses)
		{
			var jobs = new ChunkMeshGenerator[blockAddresses.Length];
			for (var i = 0; i < blockAddresses.Length; i++)
			{
				var addressByNeighbour = new NativeArray<IntPtr>(6, Allocator.TempJob);
				// order: up = 0, down = 1, front = 2, back = 3, right = 4, left = 5.
				if (TryGetUpChunkNumber(i, out var upChunkNumber))
				{
					addressByNeighbour[0] = (IntPtr) blockAddresses[upChunkNumber];
				}
				
				if (TryGetDownChunkNumber(i, out var downChunkNumber))
				{
					addressByNeighbour[1] =
						(IntPtr) blockAddresses[downChunkNumber];
				}
				
				if (TryGetFrontChunkNumber(i, out var frontChunkNumber))
				{
					addressByNeighbour[2] = (IntPtr) blockAddresses[frontChunkNumber];
				}
				
				if (TryGetBackChunkNumber(i, out var backChunkNumber))
				{
					addressByNeighbour[3] = (IntPtr) blockAddresses[backChunkNumber];
				}
				
				if (TryGetRightChunkNumber(i, out var rightChunkNumber))
				{
					addressByNeighbour[4] = (IntPtr) blockAddresses[rightChunkNumber];
				}
				
				if (TryGetLeftChunkNumber(i, out var leftChunkNumber))
				{
					addressByNeighbour[5] = (IntPtr) blockAddresses[leftChunkNumber];
				}
				
				jobs[i] = new ChunkMeshGenerator
				{
					Blocks = new NativeArray<BlockData>(_mapData.GetChunkDataByIndex(i).Blocks, Allocator.TempJob),
					Faces = new NativeArray<Faces>(ChunkData.ChunkSizeCubed, Allocator.TempJob),
					Vertices = new NativeList<Vector3>(Allocator.TempJob),
					Triangles = new NativeList<int>(Allocator.TempJob),
					Colors = new NativeList<Color32>(Allocator.TempJob),
					Normals = new NativeList<Vector3>(Allocator.TempJob),
					AddressByNeighbour = addressByNeighbour
				};
			}

			return jobs;
		}

		private NativeArray<JobHandle> RunJobs(ChunkMeshGenerator[] jobs)
		{
			var jobHandles = new NativeArray<JobHandle>(jobs.Length, Allocator.Temp);
			for (var i = 0; i < jobHandles.Length; i++)
			{
				jobHandles[i] = jobs[i].Schedule();
			}
			
			JobHandle.CompleteAll(jobHandles);
			return jobHandles;
		}

		private Chunk[] CreateChunkMeshes(ChunkMeshGenerator[] jobs, Transform chunkContainer)
		{
			var i = 0;
			var chunks = new Chunk[jobs.Length];
			for (var x = 0; x < _mapData.Width / ChunkData.ChunkSize; x++)
			{
				for (var y = 0; y < _mapData.Height / ChunkData.ChunkSize; y++)
				{
					for (var z = 0; z < _mapData.Depth / ChunkData.ChunkSize; z++)
					{
						var meshData = new MeshData(NativeListToList(jobs[i].Vertices), NativeListToList(jobs[i].Triangles),
							NativeListToList(jobs[i].Colors),
							NativeListToList(jobs[i].Normals));
						var chunkObject = _mapFactory.CreateChunk(
							new Vector3(x * ChunkData.ChunkSize, y * ChunkData.ChunkSize, z * ChunkData.ChunkSize),
							Quaternion.identity, chunkContainer);
						chunks[i] = new Chunk(chunkObject, _mapData.GetChunkDataByIndex(i), meshData, jobs[i].Faces.ToArray());
						i += 1;
					}
				}
			}

			SetChunkNeighbours(chunks);
			return chunks;
		}

		private List<T> NativeListToList<T>(NativeList<T> nativeList) where T : unmanaged
		{
			return nativeList.AsArray().ToList();
		}

		private void DisposeJobs(ChunkMeshGenerator[] jobs)
		{
			for (var i = 0; i < jobs.Length; i++)
			{
				jobs[i].AddressByNeighbour.Dispose();
				jobs[i].Blocks.Dispose();
				jobs[i].Faces.Dispose();
				jobs[i].Vertices.Dispose();
				jobs[i].Triangles.Dispose();
				jobs[i].Colors.Dispose();
				jobs[i].Normals.Dispose();
			}
		}

		private bool TryGetFrontChunkNumber(int chunkNumber, out int neighbourChunkNumber)
		{
			var notValidatedChunkNumber = chunkNumber + 1;
			if (notValidatedChunkNumber < _mapData.ChunkCount &&
			    chunkNumber / (_mapData.Depth / ChunkData.ChunkSize) ==
			    notValidatedChunkNumber / (_mapData.Depth / ChunkData.ChunkSize))
			{
				neighbourChunkNumber = notValidatedChunkNumber;
				return true;
			}

			neighbourChunkNumber = -1;
			return false;
		}

		private bool TryGetBackChunkNumber(int chunkNumber, out int neighbourChunkNumber)
		{
			var notValidatedChunkNumber = chunkNumber - 1;
			if (notValidatedChunkNumber >= 0 && chunkNumber / (_mapData.Depth / ChunkData.ChunkSize) ==
			    notValidatedChunkNumber / (_mapData.Depth / ChunkData.ChunkSize))
			{
				neighbourChunkNumber = notValidatedChunkNumber;
				return true;
			}

			neighbourChunkNumber = -1;
			return false;
		}

		private bool TryGetUpChunkNumber(int chunkNumber, out int neighbourChunkNumber)
		{
			var notValidatedChunkNumber = chunkNumber + _mapData.Depth / ChunkData.ChunkSize;
			if (notValidatedChunkNumber < _mapData.ChunkCount &&
			    chunkNumber / (_mapData.Depth / ChunkData.ChunkSize * _mapData.Height /
			                   ChunkData.ChunkSize) ==
			    notValidatedChunkNumber /
			    (_mapData.Depth / ChunkData.ChunkSize * _mapData.Height /
			     ChunkData.ChunkSize))
			{
				neighbourChunkNumber = notValidatedChunkNumber;
				return true;
			}

			neighbourChunkNumber = -1;
			return false;
		}

		private bool TryGetDownChunkNumber(int chunkNumber, out int neighbourChunkNumber)
		{
			var notValidatedChunkNumber = chunkNumber - _mapData.Depth / ChunkData.ChunkSize;
			if (notValidatedChunkNumber >= 0 &&
			    chunkNumber / (_mapData.Depth / ChunkData.ChunkSize * _mapData.Height /
			                   ChunkData.ChunkSize) ==
			    notValidatedChunkNumber /
			    (_mapData.Depth / ChunkData.ChunkSize * _mapData.Height /
			     ChunkData.ChunkSize))
			{
				neighbourChunkNumber = notValidatedChunkNumber;
				return true;
			}

			neighbourChunkNumber = -1;
			return false;
		}

		private bool TryGetRightChunkNumber(int chunkNumber, out int neighbourChunkNumber)
		{
			var notValidatedChunkNumber =
				chunkNumber + _mapData.Height / ChunkData.ChunkSize * _mapData.Depth / ChunkData.ChunkSize;
			if (notValidatedChunkNumber < _mapData.ChunkCount)
			{
				neighbourChunkNumber = notValidatedChunkNumber;
				return true;
			}

			neighbourChunkNumber = -1;
			return false;
		}

		private bool TryGetLeftChunkNumber(int chunkNumber, out int neighbourChunkNumber)
		{
			var notValidatedChunkNumber =
				chunkNumber - _mapData.Height / ChunkData.ChunkSize * _mapData.Depth /
				ChunkData.ChunkSize;
			if (notValidatedChunkNumber >= 0)
			{
				neighbourChunkNumber = notValidatedChunkNumber;
				return true;
			}

			neighbourChunkNumber = -1;
			return false;
		}
	}
}