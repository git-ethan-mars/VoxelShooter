using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using MapLogic;
using Networking.ServerServices;
using UnityEngine;

namespace Tests.EditMode
{
    public class MockMapUpdater : IMapUpdater
    {
        private readonly IMapProvider _mapProvider;
        private readonly MapHistory _mapHistory;
        private readonly ColumnDestructionAlgorithm _destructionAlgorithm;
        private readonly MapMeshUpdater _mapMeshUpdater;
        public event Action MapUpdated;

        public MockMapUpdater(IMapProvider mapProvider, MapHistory mapHistory, MapMeshUpdater mapMeshUpdater)
        {
            _mapProvider = mapProvider;
            _mapHistory = mapHistory;
            _destructionAlgorithm = new ColumnDestructionAlgorithm(mapProvider);
            _mapMeshUpdater = mapMeshUpdater;
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
            
            foreach (var fallingPosition in fallingPositions)
            {
                _mapHistory.UpdateHistory(new BlockDataWithPosition(fallingPosition, new BlockData()));
                _mapProvider.SetBlockByGlobalPosition(fallingPosition, new BlockData());
            }
            
            var updatedBlocks = new List<BlockDataWithPosition>(blocks.Count + fallingPositions.Length);
            updatedBlocks.AddRange(blocks);
            updatedBlocks.AddRange(fallingPositions.Select(fallingPosition =>
                new BlockDataWithPosition(fallingPosition, new BlockData())));
            _mapMeshUpdater.UpdateMesh(updatedBlocks.ToArray());
            MapUpdated?.Invoke();
        }
    }
}