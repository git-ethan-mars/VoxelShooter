using System.Collections.Generic;
using Data;
using Explosions;
using Infrastructure.Services.StaticData;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class BlockHealthSystem
    {
        private static Color32 _destructedBlockColor = new Color32(0,0,0,255);
        private readonly int _blockFullHealth;
        private readonly int _damagedBlockHealthThreshold;
        private readonly int _wreckedBlockHealthThreshold;
        private readonly float _fullHealthColorCoefficient;
        private readonly float _damagedColorCoefficient;
        private readonly float _wreckedColorCoefficient;

        private readonly int[] _healthByBlock;
        private readonly MapProvider _mapProvider;
        private readonly MapUpdater _mapUpdater;
        private readonly IBlockDamageCalculator _blockDamageCalculator = new LinearBlockDamageCalculator();

        public BlockHealthSystem(IStaticDataService staticData, MapProvider mapProvider, MapUpdater mapUpdater)
        {
            _mapProvider = mapProvider;
            _mapUpdater = mapUpdater;

            var healthBalance = staticData.GetBlockHealthBalance();
            _blockFullHealth = healthBalance.BlockFullHealth;
            _damagedBlockHealthThreshold = healthBalance.DamagedBlockHealthThreshold;
            _wreckedBlockHealthThreshold = healthBalance.WreckedBlockHealthThreshold;
            _fullHealthColorCoefficient = healthBalance.FullHealthColorCoefficient;
            _damagedColorCoefficient = healthBalance.DamagedColor;
            _wreckedColorCoefficient = healthBalance.WreckedColor;

            _healthByBlock = new int[_mapProvider.BlockCount];
            for (var i = 0; i < _healthByBlock.Length; i++)
            {
                _healthByBlock[i] = _blockFullHealth;
            }
        }

        public void InitializeBlocks(List<BlockDataWithPosition> blocks)
        {
            foreach (var block in blocks)
            {
                var blockIndex = GetBlockIndex(block.Position);
                _healthByBlock[blockIndex] = _blockFullHealth;
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
            if (_healthByBlock[blockIndex] >= _damagedBlockHealthThreshold)
            {
                return new BlockData(Color32.Lerp(_destructedBlockColor, blockData.Color, _fullHealthColorCoefficient));
            }

            if (_healthByBlock[blockIndex] >= _wreckedBlockHealthThreshold)
            {
                return new BlockData(Color32.Lerp(_destructedBlockColor, blockData.Color,
                    previousHealth >= _damagedBlockHealthThreshold
                        ? _damagedColorCoefficient
                        : _fullHealthColorCoefficient));
            }

            if (_healthByBlock[blockIndex] > 0)
            {
                return new BlockData(Color32.Lerp(_destructedBlockColor, blockData.Color,
                    previousHealth >= _wreckedBlockHealthThreshold
                        ? _wreckedColorCoefficient
                        : _fullHealthColorCoefficient));
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