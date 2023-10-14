using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using MapLogic;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class MapUpdater
    {
        public event Action MapUpdated;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly MapProvider _mapProvider;
        private readonly ColumnDestructionAlgorithm _destructionAlgorithm;
        private readonly BlockSplitter _blockSplitter;

        public MapUpdater(ICoroutineRunner coroutineRunner, MapProvider mapProvider,
            ColumnDestructionAlgorithm destructionAlgorithm)
        {
            _coroutineRunner = coroutineRunner;
            _mapProvider = mapProvider;
            _destructionAlgorithm = destructionAlgorithm;
            _blockSplitter = new BlockSplitter();
        }

        public void UpdateSpawnPoint(SpawnPointData oldPosition, SpawnPointData position)
        {
            var index = _mapProvider.SceneData.SpawnPoints.FindIndex(point => point.Equals(oldPosition));
            _mapProvider.SceneData.SpawnPoints[index] = position;
        }

        public void SetBlockByGlobalPosition(Vector3Int position, BlockData blockData)
        {
            if (!blockData.IsSolid())
            {
                Debug.LogWarning("This method doesn't support empty blocks");
                return;
            }

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
                if (!blockData[i].IsSolid())
                {
                    Debug.LogWarning("This method doesn't support empty blocks");
                    return;
                }

                _mapProvider.MapData
                        .Chunks[
                            _mapProvider.GetChunkNumberByGlobalPosition(positions[i].x, positions[i].y,
                                positions[i].z)]
                        .Blocks[
                            positions[i].x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                            positions[i].y % ChunkData.ChunkSize * ChunkData.ChunkSize +
                            positions[i].z % ChunkData.ChunkSize] =
                    blockData[i];
            }

            MapUpdated?.Invoke();
            var updateMessages =
                _blockSplitter.SplitArraysIntoMessages(positions.ToArray(), blockData.ToArray(), Constants.MessageSize);
            _coroutineRunner.StartCoroutine(_blockSplitter.SendMessages(updateMessages, Constants.MessageDelay));
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

            var fallingPositions = _destructionAlgorithm.Remove(positions);
            var colors = new Color32[fallingPositions.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                colors[i] = _mapProvider.GetBlockByGlobalPosition(fallingPositions[i]).Color;
            }

            for (var i = 0; i < fallingPositions.Length; i++)
            {
                _mapProvider.MapData
                        .Chunks[
                            _mapProvider.GetChunkNumberByGlobalPosition(fallingPositions[i].x, fallingPositions[i].y,
                                fallingPositions[i].z)]
                        .Blocks[
                            fallingPositions[i].x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                            fallingPositions[i].y % ChunkData.ChunkSize * ChunkData.ChunkSize +
                            fallingPositions[i].z % ChunkData.ChunkSize] =
                    new BlockData();
            }

            MapUpdated?.Invoke();
            var destroyedBlocks = positions.Union(fallingPositions).ToArray();
            var updateMessages = _blockSplitter.SplitArraysIntoMessages(destroyedBlocks,
                new BlockData[destroyedBlocks.Length],
                Constants.MessageSize);
            _coroutineRunner.StartCoroutine(_blockSplitter.SendMessages(updateMessages, Constants.MessageDelay));
            var fallingBlockMessages =
                _blockSplitter.SplitArraysIntoMessages(fallingPositions, colors, Constants.MessageSize);
            _coroutineRunner.StartCoroutine(_blockSplitter.SendMessages(fallingBlockMessages, Constants.MessageDelay));
        }
    }
}