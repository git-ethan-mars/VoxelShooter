using System;
using System.Collections.Generic;
using R3;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Pool;
namespace VoxelMap
{
	public class Map : MonoBehaviour
	{
		public static readonly Vector3 WorldOffset = new Vector3(0.5f, 0.5f, 0.5f);

		private readonly HashSet<Chunk> _regeneratingChunks = new HashSet<Chunk>();
		private readonly List<IMapFeature> _features = new List<IMapFeature>();

		private MapData _mapData;
		private Chunk[] _chunks;

		public void Construct(MapData mapData, Chunk[] chunks)
		{
			_mapData = mapData;
			_chunks = chunks;
			MapData = new MapData.Readonly(mapData);
		}

		public int Width => _mapData.Width;
		public int Height => _mapData.Height;
		public int Depth => _mapData.Depth;
		public IReadOnlyList<Chunk> Chunks => _chunks;
		public Observable<Chunk> ChunkUpdated => _chunkUpdated;
		public Observable<Unit> MapUpdated => _mapUpdated;
		public MapData.Readonly MapData { get; private set; }
		private readonly Subject<Chunk> _chunkUpdated = new Subject<Chunk>();
		private readonly Subject<Unit> _mapUpdated = new Subject<Unit>();

		private void Update()
		{
			RegenerateChunks();
		}

		private void OnDestroy()
		{
			_mapData.Dispose();
		}

		private void RegenerateChunks()
		{
			foreach (Chunk chunk in _regeneratingChunks)
			{
				chunk.Regenerate();
				_chunkUpdated.OnNext(chunk);
			}

			if (_regeneratingChunks.Count <= 0)
			{
				return;
			}
			
			_regeneratingChunks.Clear();
			_mapUpdated.OnNext(Unit.Default);

		}
		
		public void AddMapFeature(IMapFeature feature)
		{
			feature.OnAdd();
			_features.Add(feature);
		}

		public bool TryGetMapFeature<T>(out T mapFeature) where T : IMapFeature
		{
			mapFeature = default;
			IMapFeature foundFeature = _features.FirstOrDefault(feature => feature.GetType() == typeof(T));
			if (foundFeature == null)
			{
				return false;
			}

			mapFeature = (T)foundFeature;
			return true;
		}

		public VoxelData GetVoxelByGlobalPosition(int x, int y, int z)
		{
			return _mapData[x, y, z];
		}

		public VoxelData GetVoxelByGlobalPosition(Vector3Int position)
		{
			return GetVoxelByGlobalPosition(position.x, position.y, position.z);
		}

		public void SetVoxelsByGlobalPositions(List<Voxel> voxels)
		{
			using var pooledObject = DictionaryPool<int, NativeList<Voxel>>.Get(out var changedByChunkIndex);
			
			foreach (Voxel voxel in voxels)
			{
				AssertPosition(voxel.Position.x, voxel.Position.y, voxel.Position.z);
				int chunkIndex = _mapData.GetChunkIndex(voxel.Position.x, voxel.Position.y, voxel.Position.z);
				if (!changedByChunkIndex.ContainsKey(chunkIndex))
				{
					changedByChunkIndex[chunkIndex] = new NativeList<Voxel>(Allocator.TempJob);
				}

				changedByChunkIndex[chunkIndex].Add(voxel);
			}

			foreach ((int chunkNumber, var changes) in changedByChunkIndex)
			{
				RefreshFaces(chunkNumber, changes, _regeneratingChunks);
				changes.Dispose();
			}
		}

		public bool IsInsideMap(int x, int y, int z)
		{
			return x >= 0 && x < Width &&
			       y >= 0 && y < Height &&
			       z >= 0 && z < Depth;
		}
		
		public bool HasIntersection(Bounds bounds)
		{
			for (float x = bounds.min.x; x < bounds.max.x; x++)
			{
				for (float y = bounds.min.y; y < bounds.max.y; y++)
				{
					for (float z = bounds.min.z; z < bounds.max.z; z++)
					{
						if (GetVoxelByGlobalPosition((int)x, (int)y, (int)z).IsSolid())
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public bool TryGetRandomTopVoxelPosition(out Vector3Int position)
		{
			int x = UnityEngine.Random.Range(0, _mapData.Width);
			int z = UnityEngine.Random.Range(0, _mapData.Depth);

			for (int y = _mapData.Height - 1; y >= 0; y--)
			{
				if (_mapData.GetFace(x, y, z).HasFlag(Face.Top))
				{
					position = new Vector3Int(x, y, z);
					return true;
				}
			}

			position = Vector3Int.zero;
			return false;
		}

		public int GetTopVoxelHeight(int x, int z)
		{
			for (int y = _mapData.Height - 1; y >= 0; y--)
			{
				if (_mapData.GetFace(x, y, z).HasFlag(Face.Top))
				{
					return y;
				}
			}
			
			throw new InvalidOperationException($"Couldn't find top voxel at x={x}, z={z}");
		}

		private void AssertPosition(int x, int y, int z)
		{
			if (!IsInsideMap(x, y, z))
			{
				throw new ArgumentException($"{nameof(x)}={x} {nameof(y)}={y} {nameof(z)}={z} is not valid position");
			}
		}

		private void RefreshFaces(int chunkIndex, NativeList<Voxel> voxels, HashSet<Chunk> regeneratingChunks)
		{
			using NativeArray<Face> neighboursToRegenerate = new NativeArray<Face>(1, Allocator.TempJob);
			using NativeHashMap<int, int> faceCountChangesByChunk = new NativeHashMap<int, int>(1, Allocator.TempJob);
			var recalculateFacesJob = new RecalculateFacesJob(voxels, _mapData, chunkIndex, neighboursToRegenerate, faceCountChangesByChunk);
			recalculateFacesJob.Schedule().Complete();
			
			regeneratingChunks.Add(Chunks[chunkIndex]);

			foreach (var kvp in faceCountChangesByChunk)
			{
				Chunks[kvp.Key].FaceCount += kvp.Value;
			}

			if (neighboursToRegenerate[0].HasFlag(Face.Right) && TryGetRightChunk(chunkIndex, out Chunk rightChunk))
			{
				regeneratingChunks.Add(rightChunk);
			}

			if (neighboursToRegenerate[0].HasFlag(Face.Left) && TryGetLeftChunk(chunkIndex, out Chunk leftChunk))
			{
				regeneratingChunks.Add(leftChunk);
			}

			if (neighboursToRegenerate[0].HasFlag(Face.Top) && TryGetTopChunk(chunkIndex, out Chunk topChunk))
			{
				regeneratingChunks.Add(topChunk);
			}

			if (neighboursToRegenerate[0].HasFlag(Face.Bottom) && TryGetBottomChunk(chunkIndex, out Chunk bottomChunk))
			{
				regeneratingChunks.Add(bottomChunk);
			}

			if (neighboursToRegenerate[0].HasFlag(Face.Front) && TryGetFrontChunk(chunkIndex, out Chunk frontChunk))
			{
				regeneratingChunks.Add(frontChunk);
			}

			if (neighboursToRegenerate[0].HasFlag(Face.Back) && TryGetBackChunk(chunkIndex, out Chunk backChunk))
			{
				regeneratingChunks.Add(backChunk);
			}
		}

		private bool TryGetTopChunk(int chunkIndex, out Chunk chunk)
		{
			chunk = null;
			int topChunkNeighbourIndex = chunkIndex + _mapData.Depth / Chunk.ChunkSize;

			if (topChunkNeighbourIndex < Chunks.Count &&
			    chunkIndex / (_mapData.Depth / Chunk.ChunkSize * _mapData.Height / Chunk.ChunkSize) == topChunkNeighbourIndex / (_mapData.Depth /
				    Chunk.ChunkSize *
				    _mapData.Height /
				    Chunk.ChunkSize))
			{
				chunk = Chunks[topChunkNeighbourIndex];
				return true;
			}

			return false;
		}

		private bool TryGetBottomChunk(int chunkIndex, out Chunk chunk)
		{
			chunk = null;
			int bottomChunkNeighbourIndex = chunkIndex - _mapData.Depth / Chunk.ChunkSize;

			if (bottomChunkNeighbourIndex >= 0 &&
			    chunkIndex / (_mapData.Depth / Chunk.ChunkSize * _mapData.Height / Chunk.ChunkSize) ==
			    bottomChunkNeighbourIndex / (_mapData.Depth / Chunk.ChunkSize * _mapData.Height / Chunk.ChunkSize))
			{
				chunk = Chunks[bottomChunkNeighbourIndex];
				return true;
			}

			return false;
		}

		private bool TryGetFrontChunk(int chunkIndex, out Chunk chunk)
		{
			chunk = null;
			int frontChunkNeighbourIndex = chunkIndex + 1;

			if (frontChunkNeighbourIndex < Chunks.Count &&
			    chunkIndex / (_mapData.Depth / Chunk.ChunkSize) == frontChunkNeighbourIndex / (_mapData.Depth / Chunk.ChunkSize))
			{
				chunk = Chunks[frontChunkNeighbourIndex];
				return true;
			}

			return false;
		}

		private bool TryGetBackChunk(int chunkIndex, out Chunk chunk)
		{
			chunk = null;
			int backChunkNeighbourIndex = chunkIndex - 1;

			if (backChunkNeighbourIndex >= 0 &&
			    chunkIndex / (_mapData.Depth / Chunk.ChunkSize) == backChunkNeighbourIndex / (_mapData.Depth / Chunk.ChunkSize))
			{
				chunk = Chunks[backChunkNeighbourIndex];
				return true;
			}

			return false;
		}

		private bool TryGetRightChunk(int chunkIndex, out Chunk chunk)
		{
			chunk = null;
			int rightChunkNeighbourIndex = chunkIndex + _mapData.Height / Chunk.ChunkSize * _mapData.Depth / Chunk.ChunkSize;

			if (rightChunkNeighbourIndex < Chunks.Count)
			{
				chunk = Chunks[rightChunkNeighbourIndex];
				return true;
			}

			return false;
		}

		private bool TryGetLeftChunk(int chunkIndex, out Chunk chunk)
		{
			chunk = null;
			int leftChunkNeighbourIndex = chunkIndex - _mapData.Height / Chunk.ChunkSize * _mapData.Depth / Chunk.ChunkSize;

			if (leftChunkNeighbourIndex >= 0)
			{
				chunk = Chunks[leftChunkNeighbourIndex];
				return true;
			}

			return false;
		}
	}
}