using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VoxelMap
{
    public class MapProvider
    {
        public event Action MapUpdated;
        public int Width => _map.Data.Width;
        public int Height => _map.Data.Height;
        public int Depth => _map.Data.Depth;
        
        private readonly Dictionary<Chunk, List<Voxel>> _changesByChunk = new();
        private readonly Map _map;
        private readonly List<IMapFeature> _features;

        public MapProvider(Map map)
        {
            _map = map;
        }

        public void AddMapFeature(IMapFeature feature)
        {
            _features.Add(feature);
        }

        public bool TryGetMapFeature<T>(out T mapFeature) where T : IMapFeature
        {
            mapFeature = default;
            var foundFeature = _features.FirstOrDefault(feature => feature.GetType() == typeof(T));
            if (foundFeature == null)
            {
                return false;
            }

            mapFeature = (T)foundFeature;
            return true;
        }

        public VoxelData GetVoxelByGlobalPosition(int x, int y, int z)
        {
            AssertPosition(x, y, z);
            var chunk = _map.Data.GetChunkByGlobalPosition(x, y, z);
            return chunk.GetVoxel(x, y, z, PositionType.Global);
        }

        public VoxelData GetVoxelByGlobalPosition(Vector3Int position)
        {
            return GetVoxelByGlobalPosition(position.x, position.y, position.z);
        }

        public void SetVoxelByGlobalPosition(Voxel voxel)
        {
            AssertPosition(voxel.Position.x, voxel.Position.y, voxel.Position.z);
            UpdateVoxel(voxel);
            ApplyChanges();

            MapUpdated?.Invoke();
        }
        
        public void SetVoxelsByGlobalPositions(List<Voxel> voxels)
        {
            foreach (var voxel in voxels)
            {
                AssertPosition(voxel.Position.x, voxel.Position.y, voxel.Position.z);
                UpdateVoxel(voxel);
            }
            
            ApplyChanges();

            MapUpdated?.Invoke();
        }

        public void UpdateVoxel(Voxel voxel)
        {
            var chunk = _map.Chunks[_map.Data.GetChunkNumberByGlobalPosition(voxel.Position.x, voxel.Position.y, voxel.Position.z)];
            if (!_changesByChunk.ContainsKey(chunk))
            {
                _changesByChunk[chunk] = new List<Voxel>();
            }

            _changesByChunk[chunk].Add(voxel);
        }

        public void ApplyChanges()
        {
            foreach (var (chunk, changes) in _changesByChunk)
            {
                chunk.ChangeVoxels(changes);
            }

            _changesByChunk.Clear();
        }

        public byte[] Serialize()
        {
            return _map.Data.Serialize();
        }

        public bool IsInsideMap(int x, int y, int z)
        {
            return x >= 0 && x < Width &&
                   y >= 0 && y < Height &&
                   z >= 0 && z < Depth;
        }

        private void AssertPosition(int x, int y, int z)
        {
            if (!IsInsideMap(x, y, z))
            {
                throw new ArgumentException($"{nameof(x)}={x} {nameof(y)}={y} {nameof(z)}={z} is not valid position");
            }
        }
    }
}