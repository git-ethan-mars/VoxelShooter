using System;
using Data;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class MapUpdater
    {
        public event Action MapUpdated;
        private readonly MapProvider _mapProvider;

        public MapUpdater(MapProvider mapProvider)
        {
            _mapProvider = mapProvider;
        }

        public void UpdateSpawnPoint(SpawnPointData oldPosition, SpawnPointData position)
        {
            var index = _mapProvider.SceneData.SpawnPoints.FindIndex(point => point.Equals(oldPosition));
            _mapProvider.SceneData.SpawnPoints[index] = position;
        }

        public void SetBlockByGlobalPosition(Vector3Int position, BlockData blockData) =>
            SetBlockByGlobalPosition(position.x, position.y, position.z, blockData);

        private void SetBlockByGlobalPosition(int x, int y, int z, BlockData blockData)
        {
            _mapProvider.MapData.Chunks[_mapProvider.GetChunkNumberByGlobalPosition(x, y, z)]
                .Blocks[
                    x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                    y % ChunkData.ChunkSize * ChunkData.ChunkSize + z % ChunkData.ChunkSize] = blockData;
        }

        public void UpdateEntityPositions()
        {
            MapUpdated?.Invoke();
        }
    }
}