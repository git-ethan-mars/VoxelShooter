using System.Collections.Generic;
using Data;
using Explosions;
using MapLogic;
using UnityEngine;

namespace Networking.ServerServices
{
    public class BlockHealthSystem
    {
        private int[] _blocks;
        private readonly int _height;
        private readonly int _width;
        private readonly int _depth;
        private readonly MapProvider _mapProvider;
        private readonly MapUpdater _mapUpdater;
        private const int FullHealthBlockThreshold = 60;
        private const int DamagedBlockThreshold = 40;
        private const int WreckedBlockThreshold = 20;
        private const float FullHealthBlockLerp = 1;
        private const float DamagedBlockLerp= 0.7f;
        private const float WreckedBlockLerp = 0.5f;
        private IBlockDamageCalculator _blockDamageCalculator = new LinearBlockDamageCalculator();

        public BlockHealthSystem(MapProvider mapProvider, MapUpdater mapUpdater)
        {
            _mapProvider = mapProvider;
            _mapUpdater = mapUpdater;
            _height = mapProvider.MapData.Height;
            _width = mapProvider.MapData.Width;
            _depth = mapProvider.MapData.Depth;
            _blocks = new int[_height * _width * _width];
            for (var i = 0; i < _height * _width * _width; i++) 
                _blocks[i] = FullHealthBlockThreshold;
        }

        public void InitializeBlocks(List<Vector3Int> positions, List<BlockData> blockData)
        {
            foreach (var position in positions)
            {
                var blockIndex = GetBlockIndex(position);
                _blocks[blockIndex] = FullHealthBlockThreshold;
            }
            _mapUpdater.SetBlocksByGlobalPositions(positions, blockData);
        }

        public void DamageBlock(Vector3Int targetBlock, int radius, int damage, IDamageArea lineDamageArea)
        {
            var damagedBlocks = lineDamageArea.GetDamagedBlocks(radius, targetBlock);
            var damageList = _blockDamageCalculator.Calculate(targetBlock, radius, damage, damagedBlocks);

            var changedBlocksData = new List<BlockData>();
            var changedBlockPositions = new List<Vector3Int>();
            var destroyedBlockPositions = new List<Vector3Int>();
            for (var i = 0; i < damagedBlocks.Count; i++)
            {
                var blockData = _mapProvider.GetBlockByGlobalPosition(damagedBlocks[i]);
                var blockIndex = GetBlockIndex(damagedBlocks[i]);
                var previousHealth = _blocks[blockIndex];
                _blocks[blockIndex] -= damageList[i];
                var newBlockData = CalculateBlockColor(blockIndex, blockData, previousHealth);
                if (!newBlockData.Color.Equals(blockData.Color) && _blocks[blockIndex] > 0)
                {
                    changedBlocksData.Add(newBlockData);
                    changedBlockPositions.Add(damagedBlocks[i]);
                }
                if (_blocks[blockIndex] <= 0)
                    destroyedBlockPositions.Add(damagedBlocks[i]);
            }
            
            _mapUpdater.SetBlocksByGlobalPositions(changedBlockPositions, changedBlocksData);
            _mapUpdater.RemoveBlocks(destroyedBlockPositions);
        }

        private BlockData CalculateBlockColor(int blockIndex, BlockData blockData, int previousHealth)
        {
            var newBlockData = new BlockData(BlockColor.empty);
            if (_blocks[blockIndex] >= DamagedBlockThreshold)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color, FullHealthBlockLerp));
            }

            if (_blocks[blockIndex] >= WreckedBlockThreshold)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color, 
                    previousHealth >= DamagedBlockThreshold ? DamagedBlockLerp : FullHealthBlockLerp));
            }

            if (_blocks[blockIndex] > 0)
            {
                return new BlockData(Color32.Lerp(BlockColor.empty, blockData.Color,
                    previousHealth >= WreckedBlockThreshold ? WreckedBlockLerp : FullHealthBlockLerp));
            }

            return newBlockData;
        }

        private int GetBlockIndex(Vector3Int position)
        {
            return position.x * _depth * _height + position.y * _depth + position.z;
        }
    }
}