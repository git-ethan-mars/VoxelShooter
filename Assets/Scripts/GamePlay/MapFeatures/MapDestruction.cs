using System.Collections.Generic;
using Data;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;
namespace GamePlay.MapFeatures
{
	public class MapDestruction : MapFeature, IDamageVisitor
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
			var voxel = new Voxel(position, _mapProvider.Map.GetVoxelByGlobalPosition(position));

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

					var voxel = new Voxel(position, _mapProvider.Map.GetVoxelByGlobalPosition(position));

					if (IsDestructible(voxel))
					{
						voxels.Add(voxel);
					}
				}
			}
			else
			{
				var centeredBlock = new Voxel(hitPosition, _mapProvider.Map.GetVoxelByGlobalPosition(hitPosition));

				if (IsDestructible(centeredBlock))
				{
					voxels.Add(centeredBlock);
				}
			}

			HandleVoxels(voxels, constantDamageCalculator);
			
			var block = meleeWeapon.netIdentity.connectionToClient.identity.GetComponent<Character>().Inventory.GetItem<Block>();

			if (block != null)
			{
				block.Amount.Value += voxels.Count;
			}
			
			ListPool<Voxel>.Release(voxels);
		}

		public void Visit(ExplosionData explosionData, Vector3 center)
		{
			Vector3Ushort explosionCenter = Vector3Ushort.FloorToUshort(center);
			if (!_mapProvider.Map.IsInsideMap(explosionCenter.x, explosionCenter.y, explosionCenter.z))
			{
				return;
			}

			ListPool<Voxel>.Get(out var voxels);
			var damageCalculator = new SphereDamageCalculator(explosionCenter, explosionData.radius, explosionData.damage);

			for (var x = (ushort)Mathf.Max(explosionCenter.x - explosionData.radius, 0);
			     x <= Mathf.Min(explosionCenter.x + explosionData.radius,
				     _mapProvider.Map.Width);
			     x++)
			{
				for (var y = (ushort)Mathf.Max(explosionCenter.y - explosionData.radius, 0);
				     y <= Mathf.Min(explosionCenter.y + explosionData.radius, _mapProvider.Map.Height);
				     y++)
				{
					for (var z = (ushort)Mathf.Max(explosionCenter.z - explosionData.radius, 0);
					     z <= Mathf.Min(explosionCenter.z + explosionData.radius, _mapProvider.Map.Depth);
					     z++)
					{
						Vector3Ushort position = new Vector3Ushort(x, y, z);

						VoxelData blockData = _mapProvider.Map.GetVoxelByGlobalPosition(position);
						var voxel = new Voxel(position, blockData);

						if (IsDestructible(voxel) && Vector3.Distance(explosionCenter, position) < explosionData.radius)
						{
							voxels.Add(voxel);
						}
					}
				}
			}

			HandleVoxels(voxels, damageCalculator);
			ListPool<Voxel>.Release(voxels);
		}

		private void HandleVoxels(IReadOnlyList<Voxel> voxels, IVoxelDamageCalculator voxelDamageCalculator)
		{
			if (_voxelHealthSystem != null)
			{
				_voxelHealthSystem.ApplyDamage(voxels, voxelDamageCalculator);
			}
			else
			{
				_mapProvider.Map.SetVoxelsByGlobalPositions(voxels);
			}
		}

		private bool IsDestructible(Voxel voxel)
		{
			return voxel.Data.IsSolid() && voxel.Position.x < _mapProvider.Map.Width &&
			       voxel.Position.y > 0 && voxel.Position.y < _mapProvider.Map.Height && voxel.Position.z < _mapProvider.Map.Depth;
		}
	}
}