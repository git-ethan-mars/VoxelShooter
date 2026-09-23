using System.Collections.Generic;
using Data;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;
namespace GamePlay
{
	public class MapDestruction : MapFeature
	{
		private MapProvider _mapProvider;
		private VoxelHealthSystem _voxelHealthSystem;

		[Inject]
		private void Construct(MapProvider mapProvider)
		{
			_mapProvider = mapProvider;
			_voxelHealthSystem = gameObject.GetComponent<VoxelHealthSystem>();
		}

		public void Visit(RangeWeapon rangeWeapon, RaycastHit hit)
		{
			Vector3Ushort position = Vector3Ushort.FloorToUshort(hit.point - hit.normal / 2);
			var voxel = new Voxel(position, _mapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(position));

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
			Vector3Ushort hitPosition = Vector3Ushort.FloorToUshort(hit.point - hit.normal / 2);
			var constantDamageCalculator = new ConstantDamageCalculator(meleeWeapon.Configure.DamageToVoxel);
			ListPool<Voxel>.Get(out var voxels);
			if (isStrongHit)
			{
				const int length = 3;
				for (var y = (ushort)(hitPosition.y - length / 2); y <= hitPosition.y + length / 2; y++)
				{
					var position = new Vector3Ushort(hitPosition.x, y, hitPosition.z);

					var voxel = new Voxel(position, _mapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(position));

					if (IsDestructible(voxel))
					{
						voxels.Add(voxel);
					}
				}
			}
			else
			{
				var centeredBlock = new Voxel(hitPosition, _mapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(hitPosition));

				if (IsDestructible(centeredBlock))
				{
					voxels.Add(centeredBlock);
				}
			}

			HandleVoxels(voxels, constantDamageCalculator);
			
			var inventory = meleeWeapon.netIdentity.connectionToClient.identity.GetComponent<Character>().Inventory;
			inventory.VoxelAmount.Value += voxels.Count;
			
			ListPool<Voxel>.Release(voxels);
		}

		public void HandleVoxels(IReadOnlyList<Voxel> voxels, IVoxelDamageCalculator voxelDamageCalculator)
		{
			if (_voxelHealthSystem != null)
			{
				_voxelHealthSystem.ApplyDamage(voxels, voxelDamageCalculator);
			}
			else
			{
				_mapProvider.Map.CurrentValue.SetVoxelsByGlobalPositions(voxels);
			}
		}

		public bool IsDestructible(Voxel voxel)
		{
			return voxel.Data.IsSolid() && voxel.Position.x < _mapProvider.Map.CurrentValue.Width &&
			       voxel.Position.y > 0 && voxel.Position.y < _mapProvider.Map.CurrentValue.Height && voxel.Position.z < _mapProvider.Map
				   .CurrentValue.Depth;
		}
	}
}