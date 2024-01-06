using System.Collections.Generic;
using Data;
using Infrastructure.Services.StaticData;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class BlockHealthSystem
    {
        private readonly int _blockFullHealth;
        private readonly int _damagedBlockHealthThreshold;
        private readonly int _wreckedBlockHealthThreshold;
        private readonly float _fullHealthColorCoefficient;
        private readonly float _damagedColorCoefficient;
        private readonly float _wreckedColorCoefficient;

        private readonly int[] _healthByBlock;
        private readonly MapProvider _mapProvider;
        private readonly MapUpdater _mapUpdater;

        public BlockHealthSystem(IStaticDataService staticData, IServer server)
        {
            _mapProvider = server.MapProvider;
            _mapUpdater = server.MapUpdater;
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

        public BlockData DamageBlock(Vector3Int blockPosition, int damage)
        {
            var blockData = _mapProvider.GetBlockByGlobalPosition(blockPosition);
            var blockIndex = GetBlockIndex(blockPosition);
            var previousHealth = _healthByBlock[blockIndex];
            _healthByBlock[blockIndex] -= damage;
            return CalculateBlockColor(blockIndex, blockData, previousHealth);
        }

        private BlockData CalculateBlockColor(int blockIndex, BlockData blockData, int previousHealth)
        {
            var newBlockData = new BlockData(BlockColor.empty);
            if (_healthByBlock[blockIndex] >= _damagedBlockHealthThreshold)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color, _fullHealthColorCoefficient));
            }

            if (_healthByBlock[blockIndex] >= _wreckedBlockHealthThreshold)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color,
                    previousHealth >= _damagedBlockHealthThreshold
                        ? _damagedColorCoefficient
                        : _fullHealthColorCoefficient));
            }

            if (_healthByBlock[blockIndex] > 0)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color,
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