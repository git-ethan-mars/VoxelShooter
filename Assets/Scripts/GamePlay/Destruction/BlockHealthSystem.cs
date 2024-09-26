using System;
using System.Collections.Generic;
using Common.StaticData;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Destruction

{
	public class BlockHealthSystem
	{
		private static readonly Color32 DestructedBlockColor = new Color32(0, 0, 0, 255);
		private readonly int _blockFullHealth;
		private readonly int _damagedBlockHealthThreshold;
		private readonly int _wreckedBlockHealthThreshold;
		private readonly float _fullHealthColorCoefficient;
		private readonly float _damagedColorCoefficient;
		private readonly float _wreckedColorCoefficient;

		private readonly int[] _healthByBlock;
		private readonly MapProvider _mapProvider;
		private ColumnDestructionAlgorithm _destructionAlgorithm;

		public BlockHealthSystem(IStaticDataService staticData, MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
			_destructionAlgorithm = new ColumnDestructionAlgorithm(mapProvider);
			var healthBalance = staticData.GetBlockHealthBalance();
			_blockFullHealth = healthBalance.BlockFullHealth;
			_damagedBlockHealthThreshold = healthBalance.DamagedBlockHealthThreshold;
			_wreckedBlockHealthThreshold = healthBalance.WreckedBlockHealthThreshold;
			_fullHealthColorCoefficient = healthBalance.FullHealthColorCoefficient;
			_damagedColorCoefficient = healthBalance.DamagedColor;
			_wreckedColorCoefficient = healthBalance.WreckedColor;
			_healthByBlock = new int[_mapProvider.BlockCount];
			Array.Fill(_healthByBlock, _blockFullHealth);
		}

		public void InitializeBlocks(List<BlockDataWithPosition> blocks)
		{
			for (var i = 0; i < blocks.Count; i++)
			{
				SetBlockHealth(blocks[i], _blockFullHealth);
			}

			_destructionAlgorithm.Add(blocks);
		}

		public void DamageBlock(BlockDataWithPosition block, int damage)
		{
			DamageBlocks(new List<BlockDataWithPosition>(1) { block }, new ConstantDamageCalculator(damage));
		}

		public void DamageBlocks(List<BlockDataWithPosition> blocks, IBlockDamageCalculator damageCalculator)
		{
			var changedBlocks = new List<BlockDataWithPosition>();
			var destroyedBlocks = new List<BlockDataWithPosition>();
			for (var i = 0; i < blocks.Count; i++)
			{
				var previousHealth = GetBlockHealth(blocks[i]);
				var currentHealth = Math.Max(previousHealth - damageCalculator.CalculateDamage(blocks[i]), 0);
				SetBlockHealth(blocks[i], currentHealth);
				var color = CalculateBlockColor(blocks[i].BlockData, currentHealth, previousHealth);
				var damagedBlock = new BlockData(color);

				if (blocks[i].BlockData.Equals(damagedBlock))
				{
					continue;
				}

				if (damagedBlock.IsSolid())
				{
					changedBlocks.Add(new BlockDataWithPosition(blocks[i].Position, damagedBlock));
				}
				else
				{
					destroyedBlocks.Add(new BlockDataWithPosition(blocks[i].Position, damagedBlock));
				}
			}

			var fallingBlocks = _destructionAlgorithm.Remove(destroyedBlocks);
			for (var i = 0; i < fallingBlocks.Count; i++)
			{
				SetBlockHealth(fallingBlocks[i], 0);
			}

			_mapProvider.SetBlocksByGlobalPositions(changedBlocks);
			_mapProvider.SetBlocksByGlobalPositions(destroyedBlocks);
			_mapProvider.SetBlocksByGlobalPositions(fallingBlocks);
		}

		private Color32 CalculateBlockColor(BlockData blockData, int currentHealth, int previousHealth)
		{
			if (currentHealth >= _damagedBlockHealthThreshold)
			{
				return Color32.Lerp(DestructedBlockColor, blockData.Color, _fullHealthColorCoefficient);
			}

			if (currentHealth >= _wreckedBlockHealthThreshold)
			{
				return Color32.Lerp(DestructedBlockColor, blockData.Color,
					previousHealth >= _damagedBlockHealthThreshold
						? _damagedColorCoefficient
						: _fullHealthColorCoefficient);
			}

			if (currentHealth > 0)
			{
				return Color32.Lerp(DestructedBlockColor, blockData.Color,
					previousHealth >= _wreckedBlockHealthThreshold
						? _wreckedColorCoefficient
						: _fullHealthColorCoefficient);
			}

			return BlockData.Air.Color;
		}

		private int GetBlockHealth(BlockDataWithPosition block)
		{
			return _healthByBlock[block.Position.x * _mapProvider.Height * _mapProvider.Depth +
			                      block.Position.y * _mapProvider.Depth + block.Position.z];
		}

		private void SetBlockHealth(BlockDataWithPosition block, int value)
		{
			_healthByBlock[block.Position.x * _mapProvider.Height * _mapProvider.Depth +
			               block.Position.y * _mapProvider.Depth + block.Position.z] = value;
		}
	}
}