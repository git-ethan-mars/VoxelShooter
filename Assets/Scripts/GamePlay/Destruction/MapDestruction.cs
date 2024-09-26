using System.Collections.Generic;
using Common.StaticData;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Destruction
{
	public class MapDestruction : MonoBehaviour, IDamageVisitor
	{
		private MapProvider _mapProvider;
		private BlockHealthSystem _blockHealthSystem;

		public void Construct(MapProvider mapProvider, IStaticDataService staticData)
		{
			_mapProvider = mapProvider;
			_blockHealthSystem = new BlockHealthSystem(staticData, mapProvider);
		}

		public void AddBlocks(List<BlockDataWithPosition> blocks)
		{
			_blockHealthSystem.InitializeBlocks(blocks);
			_mapProvider.SetBlocksByGlobalPositions(blocks);
		}

		public void Visit(RangeWeaponData rangeWeapon, RaycastHit hit)
		{
			var position = Vector3Int.FloorToInt(hit.point - hit.normal / 2);
			var blockData = _mapProvider.GetBlockByGlobalPosition(position);
			var block = new BlockDataWithPosition(position, blockData);
			if (IsDestructible(block))
			{
				_blockHealthSystem.DamageBlock(block, rangeWeapon.Damage);
			}
		}

		public void Visit(MeleeWeaponData meleeWeapon, bool isStrongHit, RaycastHit hit)
		{
			var hitPosition = Vector3Int.FloorToInt(hit.point - hit.normal / 2);
			var constantDamageCalculator = new ConstantDamageCalculator(meleeWeapon.DamageToBlock);
			if (isStrongHit)
			{
				var length = 3;
				var blocks = new List<BlockDataWithPosition>();
				for (var i = -length / 2; i <= length / 2; i++)
				{
					var offset = new Vector3Int(0, i, 0);
					var blockPosition = hitPosition + offset;
					var block = new BlockDataWithPosition(blockPosition, _mapProvider.GetBlockByGlobalPosition(blockPosition));
					if (IsDestructible(block))
					{
						blocks.Add(block);
					}
				}

				_blockHealthSystem.DamageBlocks(blocks, constantDamageCalculator);
			}

			var centeredBlock = new BlockDataWithPosition(hitPosition, _mapProvider.GetBlockByGlobalPosition(hitPosition));
			_blockHealthSystem.DamageBlock(centeredBlock, meleeWeapon.DamageToBlock);
		}

		public void Visit(Explosion data)
		{
			var explosionCenter = Vector3Int.FloorToInt(data.transform.position);
			if (!_mapProvider.IsInsideMap(explosionCenter.x, explosionCenter.y, explosionCenter.z))
			{
				return;
			}

			var blocks = new List<BlockDataWithPosition>();
			for (var x = -data.Radius; x <= data.Radius; x++)
			{
				for (var y = -data.Radius; y <= data.Radius; y++)
				{
					for (var z = -data.Radius; z <= data.Radius; z++)
					{
						var offset = new Vector3Int(x, y, z);
						var blockPosition = explosionCenter + offset;
						if (!_mapProvider.IsInsideMap(explosionCenter.x + x, explosionCenter.y + y, explosionCenter.z + z))
						{
							continue;
						}

						var blockData = _mapProvider.GetBlockByGlobalPosition(blockPosition);
						var block = new BlockDataWithPosition(blockPosition, blockData);
						if (IsDestructible(block) && Vector3.Distance(explosionCenter, explosionCenter) <= data.Radius)
						{
							blocks.Add(block);
						}
					}
				}
			}

			var explosionCenterBlock = new BlockDataWithPosition(explosionCenter,
				_mapProvider.GetBlockByGlobalPosition(explosionCenter));
			var sphereDamageCalculator = new SphereDamageCalculator(explosionCenterBlock, data.Radius, data.Damage);
			_blockHealthSystem.DamageBlocks(blocks, sphereDamageCalculator);
		}

		private bool IsDestructible(BlockDataWithPosition block)
		{
			return block.BlockData.IsSolid() && block.Position.x >= 0 && block.Position.x < _mapProvider.Width &&
			       block.Position.y > 0 && block.Position.y < _mapProvider.Height &&
			       block.Position.z >= 0 && block.Position.z < _mapProvider.Depth;
		}
	}
}