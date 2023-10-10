using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Mirror;
using Networking.Messages.Responses;
using Networking.ServerServices;
using UnityEngine;

namespace MapLogic
{
    public class MapUpdater
    {
        public event Action MapUpdated;
        private readonly MapProvider _mapProvider;
        private readonly ColumnDestructionAlgorithm _destructionAlgorithm;

        public MapUpdater(MapProvider mapProvider, ColumnDestructionAlgorithm destructionAlgorithm)
        {
            _mapProvider = mapProvider;
            _destructionAlgorithm = destructionAlgorithm;
        }

        public void UpdateSpawnPoint(SpawnPointData oldPosition, SpawnPointData position)
        {
            var index = _mapProvider.SceneData.SpawnPoints.FindIndex(point => point.Equals(oldPosition));
            _mapProvider.SceneData.SpawnPoints[index] = position;
        }

        public void SetBlockByGlobalPosition(Vector3Int position, BlockData blockData)
        {
            _mapProvider.MapData.Chunks[_mapProvider.GetChunkNumberByGlobalPosition(position.x, position.y, position.z)]
                    .Blocks[
                        position.x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                        position.y % ChunkData.ChunkSize * ChunkData.ChunkSize + position.z % ChunkData.ChunkSize] =
                blockData;

            MapUpdated?.Invoke();
            _destructionAlgorithm.Add(new List<Vector3Int> {position});
            NetworkServer.SendToAll(new UpdateMapResponse(new[] {position}, new[] {blockData}));
        }

        public void SetBlocksByGlobalPositions(List<Vector3Int> positions, List<BlockData> blockData)
        {
            for (var i = 0; i < positions.Count; i++)
            {
                _mapProvider.MapData
                        .Chunks[
                            _mapProvider.GetChunkNumberByGlobalPosition(positions[i].x, positions[i].y, positions[i].z)]
                        .Blocks[
                            positions[i].x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                            positions[i].y % ChunkData.ChunkSize * ChunkData.ChunkSize +
                            positions[i].z % ChunkData.ChunkSize] =
                    blockData[i];
            }

            MapUpdated?.Invoke();
            NetworkServer.SendToAll(new UpdateMapResponse(positions.ToArray(), blockData.ToArray()));
            _destructionAlgorithm.Add(positions);
        }

        public void DestroyBlocks(List<Vector3Int> positions)
        {
            for (var i = 0; i < positions.Count; i++)
            {
                _mapProvider.MapData
                        .Chunks[
                            _mapProvider.GetChunkNumberByGlobalPosition(positions[i].x, positions[i].y, positions[i].z)]
                        .Blocks[
                            positions[i].x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                            positions[i].y % ChunkData.ChunkSize * ChunkData.ChunkSize +
                            positions[i].z % ChunkData.ChunkSize] =
                    new BlockData();
            }

            MapUpdated?.Invoke();
            var fallingPositions = _destructionAlgorithm.Remove(positions);
            var destroyedBlocks = positions.Union(fallingPositions).ToArray();
            NetworkServer.SendToAll(new UpdateMapResponse(destroyedBlocks, new BlockData[destroyedBlocks.Length]));
            var colors = new Color32[fallingPositions.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = _mapProvider.GetBlockByGlobalPosition(fallingPositions[i]).Color;
            }

            NetworkServer.SendToAll(new FallBlockResponse(fallingPositions, colors));
        }
    }
}