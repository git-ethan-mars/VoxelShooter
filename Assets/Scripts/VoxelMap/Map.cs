using System;
using System.Collections.Generic;
using R3;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap.Data;
namespace VoxelMap
{
	public class Map : MonoBehaviour
	{
		public static readonly Vector3 WorldOffset = new Vector3(0.5f, 0.5f, 0.5f);

		private readonly HashSet<Chunk> _regeneratingChunks = new HashSet<Chunk>();

		private MapData _mapData;
		private Chunk[] _chunks;

		public void Construct(MapData mapData, Chunk[] chunks, MapConfigure mapConfigure)
		{
			_mapData = mapData;
			_chunks = chunks;
			MapData = new MapData.Readonly(mapData);
			MapConfigure = mapConfigure;
		}

		public ushort Width => _mapData.Width;
		public ushort Height => _mapData.Height;
		public ushort Depth => _mapData.Depth;
		public IReadOnlyList<Chunk> Chunks => _chunks;
		public Observable<Chunk> ChunkUpdated => _chunkUpdated;
		public Observable<Unit> MapUpdated => _mapUpdated;
		public Observable<IReadOnlyList<Voxel>> VoxelsAdded => _voxelsAdded;
		public Observable<IReadOnlyList<Vector3Ushort>> VoxelsRemoved => _voxelsRemoved;
		private readonly Subject<IReadOnlyList<Voxel>> _voxelsAdded = new Subject<IReadOnlyList<Voxel>>();
		private readonly Subject<IReadOnlyList<Vector3Ushort>> _voxelsRemoved = new Subject<IReadOnlyList<Vector3Ushort>>();
		public MapConfigure MapConfigure { get; private set; }
		public MapData.Readonly MapData { get; private set; }
		private readonly Subject<Chunk> _chunkUpdated = new Subject<Chunk>();
		private readonly Subject<Unit> _mapUpdated = new Subject<Unit>();
		private readonly Dictionary<Type, MapFeature> _features = new Dictionary<Type, MapFeature>();

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
			if (_regeneratingChunks.Count <= 0)
			{
				return;
			}

			Chunk.RegenerateParallel(_mapData, _regeneratingChunks);


			foreach (Chunk chunk in _regeneratingChunks)
			{
				_chunkUpdated.OnNext(chunk);
			}

			_regeneratingChunks.Clear();
			_mapUpdated.OnNext(Unit.Default);
		}

		public VoxelData GetVoxelByGlobalPosition(ushort x, ushort y, ushort z)
		{
			return _mapData[x, y, z];
		}

		public VoxelData GetVoxelByGlobalPosition(Vector3Ushort position)
		{
			return GetVoxelByGlobalPosition(position.x, position.y, position.z);
		}

		public void SetVoxelsByGlobalPositions(IReadOnlyList<Voxel> voxels)
		{
			using var pooledObject = DictionaryPool<int, NativeList<Voxel>>.Get(out var changedByChunkIndex);

			foreach (Voxel voxel in voxels)
			{
				Vector3Ushort position = voxel.Position;

				int chunkIndex = _mapData.GetChunkIndex(position.x, position.y, position.z);

				if (!changedByChunkIndex.ContainsKey(chunkIndex))
				{
					changedByChunkIndex[chunkIndex] = new NativeList<Voxel>(Allocator.TempJob);
				}

				changedByChunkIndex[chunkIndex].Add(voxel);
			}

			foreach ((int chunkNumber, var changes) in changedByChunkIndex)
			{
				RefreshFaceAmount(chunkNumber, changes);
				changes.Dispose();
			}

			var addedVoxels = ListPool<Voxel>.Get();
			var removedPosition = ListPool<Vector3Ushort>.Get();

			for (var i = 0; i < voxels.Count; i++)
			{
				if (voxels[i].Data.IsSolid())
				{
					addedVoxels.Add(voxels[i]);
				}
				else
				{
					removedPosition.Add(voxels[i].Position);
				}
			}

			if (addedVoxels.Count > 0)
			{
				_voxelsAdded.OnNext(addedVoxels);
			}

			if (removedPosition.Count > 0)
			{
				_voxelsRemoved.OnNext(removedPosition);
			}
			
			ListPool<Voxel>.Release(addedVoxels);
			ListPool<Vector3Ushort>.Release(removedPosition);
		}

		public bool IsInsideMap(ushort x, ushort y, ushort z)
		{
			return x < Width && y < Height && z < Depth;
		}

		public bool IsInsideMap(Vector3Ushort position)
		{
			return IsInsideMap(position.x, position.y, position.z);
		}

		public bool HasIntersection(Bounds bounds)
		{
			for (float x = bounds.min.x; x < bounds.max.x; x++)
			{
				for (float y = bounds.min.y; y < bounds.max.y; y++)
				{
					for (float z = bounds.min.z; z < bounds.max.z; z++)
					{
						if (GetVoxelByGlobalPosition((ushort)x, (ushort)y, (ushort)z).IsSolid())
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public bool TryGetRandomTopVoxelPosition(out Vector3Ushort position)
		{
			var x = (ushort)UnityEngine.Random.Range(0, _mapData.Width);
			var z = (ushort)UnityEngine.Random.Range(0, _mapData.Depth);

			ushort y = (ushort)(_mapData.Height - 1);

			if (_mapData.GetFace(x, y, z).HasFlag(Face.Top))
			{
				position = new Vector3Ushort(x, y, z);
				return true;
			}

			do
			{
				y--;

				if (_mapData.GetFace(x, y, z).HasFlag(Face.Top))
				{
					position = new Vector3Ushort(x, y, z);
					return true;
				}

			} while (y > 0);

			position = Vector3Ushort.zero;
			return false;
		}

		public void AddFeature<TFeature>() where TFeature : MapFeature
		{
			var feature = gameObject.AddComponent<TFeature>();
			_features[typeof(TFeature)] = feature;
		}

		public bool TryGetFeature<TFeature>(out TFeature feature) where TFeature : MapFeature
		{
			feature = null;

			if (_features.TryGetValue(typeof(TFeature), out MapFeature foundFeature))
			{
				feature = (TFeature)foundFeature;
				return true;
			}

			return false;
		}

		private void RefreshFaceAmount(int chunkIndex, NativeList<Voxel> voxels)
		{
			using NativeReference<Face> regeneratingNeighbours = new NativeReference<Face>(Allocator.TempJob);
			using NativeHashMap<int, int> faceCountChangesByChunk = new NativeHashMap<int, int>(1, Allocator.TempJob);
			var recalculateFacesJob = new RecalculateFacesJob(voxels, _mapData, chunkIndex, regeneratingNeighbours, faceCountChangesByChunk);
			recalculateFacesJob.Schedule().Complete();

			_regeneratingChunks.Add(Chunks[chunkIndex]);

			foreach (var kvp in faceCountChangesByChunk)
			{
				Chunks[kvp.Key].FaceCount += kvp.Value;
			}

			if (regeneratingNeighbours.Value.HasFlag(Face.Right) && TryGetRightChunk(chunkIndex, out Chunk rightChunk))
			{
				_regeneratingChunks.Add(rightChunk);
			}

			if (regeneratingNeighbours.Value.HasFlag(Face.Left) && TryGetLeftChunk(chunkIndex, out Chunk leftChunk))
			{
				_regeneratingChunks.Add(leftChunk);
			}

			if (regeneratingNeighbours.Value.HasFlag(Face.Top) && TryGetTopChunk(chunkIndex, out Chunk topChunk))
			{
				_regeneratingChunks.Add(topChunk);
			}

			if (regeneratingNeighbours.Value.HasFlag(Face.Bottom) && TryGetBottomChunk(chunkIndex, out Chunk bottomChunk))
			{
				_regeneratingChunks.Add(bottomChunk);
			}

			if (regeneratingNeighbours.Value.HasFlag(Face.Front) && TryGetFrontChunk(chunkIndex, out Chunk frontChunk))
			{
				_regeneratingChunks.Add(frontChunk);
			}

			if (regeneratingNeighbours.Value.HasFlag(Face.Back) && TryGetBackChunk(chunkIndex, out Chunk backChunk))
			{
				_regeneratingChunks.Add(backChunk);
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