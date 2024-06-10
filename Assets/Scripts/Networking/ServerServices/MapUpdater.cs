using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class MapUpdater : IMapUpdater
    {
        public event Action MapUpdated;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IMapProvider _mapProvider;
        private readonly MapHistory _mapHistory;
        private readonly ColumnDestructionAlgorithm _destructionAlgorithm;

        public MapUpdater(ICoroutineRunner coroutineRunner, IMapProvider mapProvider, MapHistory mapHistory)
        {
            _coroutineRunner = coroutineRunner;
            _mapProvider = mapProvider;
            _mapHistory = mapHistory;
            _destructionAlgorithm = new ColumnDestructionAlgorithm(mapProvider);
        }

        public void UpdateMap(List<BlockDataWithPosition> blocks)
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
                
                _mapHistory.UpdateHistory(blocks[i]);
                _mapProvider.SetBlockByGlobalPosition(blocks[i].Position, blocks[i].BlockData);
            }

            _destructionAlgorithm.Add(createdBlocks.Select(block => block.Position));
            var fallingPositions = _destructionAlgorithm.Remove(removedBlocks.Select(block => block.Position).ToList());

            SendUpdatedBlocks(blocks, fallingPositions);
            SendFallingBlocks(fallingPositions);

            foreach (var fallingPosition in fallingPositions)
            {
                _mapHistory.UpdateHistory(new BlockDataWithPosition(fallingPosition, new BlockData()));
                _mapProvider.SetBlockByGlobalPosition(fallingPosition, new BlockData());
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
                MessageSplitter.SplitBlocksIntoFallingMessages(fallingBlocks, Constants.MessageSize);
            _coroutineRunner.StartCoroutine(MessageSplitter.SendMessages(fallingBlockMessages, Constants.MessageDelay,
                true));
        }

        private void SendUpdatedBlocks(List<BlockDataWithPosition> blocks, Vector3Int[] fallingPositions)
        {
            var updatedBlocks = new List<BlockDataWithPosition>(blocks.Count + fallingPositions.Length);
            updatedBlocks.AddRange(blocks);
            updatedBlocks.AddRange(fallingPositions.Select(fallingPosition =>
                new BlockDataWithPosition(fallingPosition, new BlockData())));
            var updateMessages = MessageSplitter.SplitBlocksIntoUpdateMessages(updatedBlocks, Constants.MessageSize);
            _coroutineRunner.StartCoroutine(MessageSplitter.SendMessages(updateMessages, Constants.MessageDelay));
        }
    }
}