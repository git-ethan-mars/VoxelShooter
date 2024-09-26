using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.StaticData;
using Explosions;
using Networking.Messages.Responses;
using UnityEngine;
using VoxelMap;

namespace Networking.Host.Services
{
	public class MapUpdater
	{
		public event Action MapUpdated;
		private readonly MapProvider _mapProvider;
		private readonly BlockHealthSystem _blockHealthSystem;
		private readonly ColumnDestructionAlgorithm _destructionAlgorithm;

		public MapUpdater(IStaticDataService staticData, MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
			_blockHealthSystem = new BlockHealthSystem(staticData, mapProvider);
			_destructionAlgorithm = new ColumnDestructionAlgorithm(mapProvider);
		}

		public BlockData GetBlockByGlobalPosition(Vector3Int position)
		{
			return _mapProvider.GetBlockByGlobalPosition(position);
		}

		public BlockData GetBlockByGlobalPosition(int x, int y, int z)
		{
			return _mapProvider.GetBlockByGlobalPosition(x, y, z);
		}

		public bool IsDestructiblePosition(Vector3Int position)
		{
			return _mapProvider.IsDestructiblePosition(position);
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

				_mapProvider.SetBlockByGlobalPosition(blocks[i].Position, blocks[i].BlockData);
			}

			_blockHealthSystem.InitializeBlocks(createdBlocks);
			_destructionAlgorithm.Add(createdBlocks.Select(block => block.Position));
			var fallingPositions = _destructionAlgorithm.Remove(removedBlocks.Select(block => block.Position).ToList());

			SendUpdatedBlocks(blocks, fallingPositions);
			SendFallingBlocks(fallingPositions);

			foreach (var fallingPosition in fallingPositions)
			{
				_mapProvider.SetBlockByGlobalPosition(fallingPosition, new BlockData());
			}

			MapUpdated?.Invoke();
		}

		public void DamageBlocks(IDamageBlockArea damageArea, Vector3Int damageCenter, int baseDamage)
		{
			var overlappedPositions = damageArea.GetOverlappedBlockPositions(damageCenter);
			var changedBlocks = new List<BlockDataWithPosition>();
			for (var i = 0; i < overlappedPositions.Count; i++)
			{
				var oldBlock = GetBlockByGlobalPosition(overlappedPositions[i]);
				var damage = damageArea.CalculateBlockDamage(overlappedPositions[i], damageCenter, baseDamage);
				var newBlock = _blockHealthSystem.DamageBlock(overlappedPositions[i], damage);
				if (!oldBlock.Equals(newBlock))
				{
					changedBlocks.Add(new BlockDataWithPosition(overlappedPositions[i], newBlock));
				}
			}

			SetBlocksByGlobalPositions(changedBlocks);
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
				MessageSplitter.SplitBlocksIntoMessages<FallBlockResponse>(fallingBlocks, Constants.MessageSize);
			MessageSplitter.SendMessages(fallingBlockMessages, Constants.MessageDelay,
				true);
		}

		private void SendUpdatedBlocks(List<BlockDataWithPosition> blocks, Vector3Int[] fallingPositions)
		{
			var updatedBlocks = new List<BlockDataWithPosition>(blocks.Count + fallingPositions.Length);
			updatedBlocks.AddRange(blocks);
			updatedBlocks.AddRange(fallingPositions.Select(fallingPosition =>
				new BlockDataWithPosition(fallingPosition, new BlockData())));
			var updateMessages = MessageSplitter.SplitBlocksIntoMessages<UpdateMapResponse>(updatedBlocks,
				Constants.MessageSize);
			MessageSplitter.SendMessages(updateMessages, Constants.MessageDelay);
		}
	}
}