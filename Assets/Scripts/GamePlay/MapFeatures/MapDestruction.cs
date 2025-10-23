using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;
namespace GamePlay.MapFeatures
{
	public class MapDestruction : IMapFeature, IDamageVisitor
	{
		private readonly ColumnDestructionAlgorithm _destructionAlgorithm;
		private readonly Map _map;
		private readonly VoxelHealthSystem _voxelHealthSystem;

		public MapDestruction(Map map, VoxelHealthSystem voxelHealthSystem = null,
			ColumnDestructionAlgorithm destructionAlgorithm = null)
		{
			_map = map;
			_voxelHealthSystem = voxelHealthSystem;
			_destructionAlgorithm = destructionAlgorithm;
		}

		public void Visit(RangeWeapon rangeWeapon, RaycastHit hit)
		{
			Vector3Int position = Vector3Int.FloorToInt(hit.point - hit.normal / 2);
			var voxel = new Voxel(position, _map.GetVoxelByGlobalPosition(position));
			if (IsDestructible(voxel))
			{
				var constantDamageCalculator = new ConstantDamageCalculator(rangeWeapon.Configure.Damage);
				ListPool<Voxel>.Get(out var voxels);
				voxels.Add(voxel);
				HandleVoxels(voxels, constantDamageCalculator);
				ListPool<Voxel>.Release(voxels);
			}
		}

		public void Visit(MeleeWeapon meleeWeapon, bool isStrongHit, RaycastHit hit)
		{
			Vector3Int hitPosition = Vector3Int.FloorToInt(hit.point - hit.normal / 2);
			var constantDamageCalculator = new ConstantDamageCalculator(meleeWeapon.Configure.DamageToVoxel);
			ListPool<Voxel>.Get(out var voxels);
			if (isStrongHit)
			{
				const int length = 3;
				for (int i = -length / 2; i <= length / 2; i++)
				{
					var offset = new Vector3Int(0, i, 0);
					Vector3Int blockPosition = hitPosition + offset;
					var voxel = new Voxel(blockPosition, _map.GetVoxelByGlobalPosition(blockPosition));
					if (IsDestructible(voxel))
					{
						voxels.Add(voxel);
					}
				}
			}
			else
			{
				var centeredBlock = new Voxel(hitPosition, _map.GetVoxelByGlobalPosition(hitPosition));

				if (IsDestructible(centeredBlock))
				{
					voxels.Add(centeredBlock);
				}
			}

			HandleVoxels(voxels, constantDamageCalculator);
			ListPool<Voxel>.Release(voxels);
		}

		public void Visit(ExplosionData explosionData, Vector3 center)
		{
			Vector3Int explosionCenter = Vector3Int.FloorToInt(center);
			if (!_map.IsInsideMap(explosionCenter.x, explosionCenter.y, explosionCenter.z))
			{
				return;
			}

			ListPool<Voxel>.Get(out var voxels);
			var damageCalculator = new SphereDamageCalculator(explosionCenter, explosionData.radius, explosionData.damage);

			for (int x = -explosionData.radius; x <= explosionData.radius; x++)
			{
				for (int y = -explosionData.radius; y <= explosionData.radius; y++)
				{
					for (int z = -explosionData.radius; z <= explosionData.radius; z++)
					{
						var offset = new Vector3Int(x, y, z);
						Vector3Int blockPosition = explosionCenter + offset;
						if (!_map.IsInsideMap(blockPosition.x, blockPosition.y, blockPosition.z))
						{
							continue;
						}

						VoxelData blockData = _map.GetVoxelByGlobalPosition(blockPosition);
						var voxel = new Voxel(blockPosition, blockData);
						if (IsDestructible(voxel) && Vector3.Distance(explosionCenter, blockPosition) < explosionData.radius)
						{
							voxels.Add(voxel);
						}
					}
				}
			}

			HandleVoxels(voxels, damageCalculator);
			ListPool<Voxel>.Release(voxels);
		}

		public void OnAdd()
		{
			var damageVisitorWrapper = _map.gameObject.AddComponent<DamageVisitorWrapper>();
			damageVisitorWrapper.Construct(this);
		}

		private void HandleVoxels(List<Voxel> voxels, IVoxelDamageCalculator voxelDamageCalculator)
		{
			List<Voxel> damagedVoxels;
			if (_voxelHealthSystem != null)
			{
				damagedVoxels = _voxelHealthSystem.DamageVoxels(voxels, voxelDamageCalculator);
			}
			else
			{
				damagedVoxels = voxels;

				for (var i = 0; i < damagedVoxels.Count; i++)
				{
					damagedVoxels[i] = new Voxel(damagedVoxels[i].Position, VoxelData.Air);
				}
			}

			if (damagedVoxels.Count == 0)
			{
				return;
			}

			if (_destructionAlgorithm != null)
			{
				var destroyedVoxels = damagedVoxels.Where(damagedVoxel => !damagedVoxel.Data.IsSolid()).ToList();
				var fallingVoxels = _destructionAlgorithm.Remove(destroyedVoxels);
				ListPool<Voxel>.Get(out var result);
				result.AddRange(damagedVoxels);
				result.AddRange(fallingVoxels);
				_map.SetVoxelsByGlobalPositions(result);
				ListPool<Voxel>.Release(result);
			}
			else
			{
				_map.SetVoxelsByGlobalPositions(damagedVoxels);
			}
		}

		private bool IsDestructible(Voxel voxel)
		{
			return voxel.Data.IsSolid() && voxel.Position.x >= 0 && voxel.Position.x < _map.Width &&
			       voxel.Position.y > 0 && voxel.Position.y < _map.Height &&
			       voxel.Position.z >= 0 && voxel.Position.z < _map.Depth;
		}
	}
}