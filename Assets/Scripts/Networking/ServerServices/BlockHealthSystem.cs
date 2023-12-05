using System.Collections.Generic;
using Data;
using Explosions;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class BlockHealthSystem
    {
        private const int FullHealthBlockThreshold = 60;
        private const int DamagedBlockThreshold = 40;
        private const int WreckedBlockThreshold = 20;
        private const float FullHealthBlockLerp = 1;
        private const float DamagedBlockLerp = 0.7f;
        private const float WreckedBlockLerp = 0.5f;

        private readonly int[] _healthByBlock;
        private readonly MapProvider _mapProvider;
        private readonly MapUpdater _mapUpdater;
        private readonly IBlockDamageCalculator _blockDamageCalculator = new LinearBlockDamageCalculator();

        public BlockHealthSystem(MapProvider mapProvider, MapUpdater mapUpdater)
        {
            _mapProvider = mapProvider;
            _mapUpdater = mapUpdater;
            _healthByBlock = new int[_mapProvider.BlockCount];
            for (var i = 0; i < _healthByBlock.Length; i++)
            {
                _healthByBlock[i] = FullHealthBlockThreshold;
            }
        }

        public void InitializeBlocks(List<BlockDataWithPosition> blocks)
        {
            foreach (var block in blocks)
            {
                var blockIndex = GetBlockIndex(block.Position);
                _healthByBlock[blockIndex] = FullHealthBlockThreshold;
            }

            _mapUpdater.SetBlocksByGlobalPositions(blocks);
        }

        public void DamageBlock(Vector3Int targetBlock, int radius, int damage, IDamageArea lineDamageArea)
        {
            var damagedBlocks = lineDamageArea.GetDamagedBlocks(radius, targetBlock);
            var changedBlocks = new List<BlockDataWithPosition>();
            for (var i = 0; i < damagedBlocks.Count; i++)
            {
                var blockData = _mapProvider.GetBlockByGlobalPosition(damagedBlocks[i]);
                var blockIndex = GetBlockIndex(damagedBlocks[i]);
                var previousHealth = _healthByBlock[blockIndex];
                _healthByBlock[blockIndex] -=
                    _blockDamageCalculator.Calculate(targetBlock, radius, damage, damagedBlocks[i]);
                var newBlockData = CalculateBlockColor(blockIndex, blockData, previousHealth);
                if (!newBlockData.Color.Equals(blockData.Color) && _healthByBlock[blockIndex] > 0)
                {
                    changedBlocks.Add(new BlockDataWithPosition(damagedBlocks[i], newBlockData));
                }

                if (_healthByBlock[blockIndex] <= 0)
                {
                    _healthByBlock[blockIndex] = 0;
                    changedBlocks.Add(new BlockDataWithPosition(damagedBlocks[i], new BlockData()));
                }
            }

            _mapUpdater.SetBlocksByGlobalPositions(changedBlocks);
        }

        private BlockData CalculateBlockColor(int blockIndex, BlockData blockData, int previousHealth)
        {
            var newBlockData = new BlockData(BlockColor.empty);
            if (_healthByBlock[blockIndex] >= DamagedBlockThreshold)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color, FullHealthBlockLerp));
            }

            if (_healthByBlock[blockIndex] >= WreckedBlockThreshold)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color,
                    previousHealth >= DamagedBlockThreshold ? DamagedBlockLerp : FullHealthBlockLerp));
            }

            if (_healthByBlock[blockIndex] > 0)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color,
                    previousHealth >= WreckedBlockThreshold ? WreckedBlockLerp : FullHealthBlockLerp));
            }

            return newBlockData;
        }

        private int GetBlockIndex(Vector3Int position)
        {
            return position.x * _mapProvider.MapData.Height * _mapProvider.MapData.Depth +
                   position.y * _mapProvider.MapData.Depth + position.z;
        }
    }
}