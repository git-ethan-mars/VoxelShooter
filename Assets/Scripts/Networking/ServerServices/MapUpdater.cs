using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using MapLogic;
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

        public MapUpdater(ICoroutineRunner coroutineRunner, MapProvider mapProvider)
        {
            _coroutineRunner = coroutineRunner;
            _mapProvider = mapProvider;
            _destructionAlgorithm = new ColumnDestructionAlgorithm(mapProvider);
            _blockSplitter = new BlockSplitter();
        }

        public void SetBlocksByGlobalPositions(List<BlockDataWithPosition> blocks)
        {
            var createdBlocks = new List<BlockDataWithPosition>();
            var removedBlocks = new List<BlockDataWithPosition>();
            for (var i = 0; i < blocks.Count; i++)
            {
                if (!_mapProvider.GetBlockByGlobalPosition(blocks[i].Position).IsSolid() &&
                    blocks[i].BlockData.IsSolid())
                {
                    createdBlocks.Add(blocks[i]);
                }

                if (_mapProvider.GetBlockByGlobalPosition(blocks[i].Position).IsSolid() &&
                    !blocks[i].BlockData.IsSolid())
                {
                    removedBlocks.Add(blocks[i]);
                }

                SetBlockByGlobalPosition(blocks[i].Position, blocks[i].BlockData);
            }

            _destructionAlgorithm.Add(createdBlocks.Select(block => block.Position));
            var fallingPositions = _destructionAlgorithm.Remove(removedBlocks.Select(block => block.Position).ToList());

            SendUpdatedBlocks(blocks, fallingPositions);
            SendFallingBlocks(fallingPositions);

            foreach (var fallingPosition in fallingPositions)
            {
                SetBlockByGlobalPosition(fallingPosition, new BlockData());
            }

            MapUpdated?.Invoke();
        }

        private void SendFallingBlocks(Vector3Int[] fallingPositions)
        {
            var fallingBlocks = new BlockDataWithPosition[fallingPositions.Length];
            for (var i = 0; i < fallingBlocks.Length; i++)
            {
                fallingBlocks[i] = new BlockDataWithPosition(fallingPositions[i],
                    _mapProvider.GetBlockByGlobalPosition(fallingPositions[i]));
            }

            var fallingBlockMessages =
                _blockSplitter.SplitBlocksIntoFallingMessages(fallingBlocks, Constants.MessageSize);
            _coroutineRunner.StartCoroutine(_blockSplitter.SendMessages(fallingBlockMessages, Constants.MessageDelay));
        }

        private void SendUpdatedBlocks(List<BlockDataWithPosition> blocks, Vector3Int[] fallingPositions)
        {
            var updatedBlocks = new List<BlockDataWithPosition>(blocks.Count + fallingPositions.Length);
            updatedBlocks.AddRange(blocks);
            updatedBlocks.AddRange(fallingPositions.Select(fallingPosition =>
                new BlockDataWithPosition(fallingPosition, new BlockData())));
            var updateMessages = _blockSplitter.SplitBlocksIntoUpdateMessages(updatedBlocks, Constants.MessageSize);
            _coroutineRunner.StartCoroutine(_blockSplitter.SendMessages(updateMessages, Constants.MessageDelay));
        }

        private void SetBlockByGlobalPosition(Vector3Int position, BlockData block)
        {
            var chunkIndex = _mapProvider.GetChunkNumberByGlobalPosition(position.x, position.y, position.z);
            var blockIndex = position.x % ChunkData.ChunkSize * ChunkData.ChunkSizeSquared +
                             position.y % ChunkData.ChunkSize * ChunkData.ChunkSize +
                             position.z % ChunkData.ChunkSize;
            _mapProvider.MapData.Chunks[chunkIndex].Blocks[blockIndex] = block;
        }
    }
}