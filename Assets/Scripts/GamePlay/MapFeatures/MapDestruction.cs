using System.Collections.Generic;
using System.Linq;
using GamePlay.Data;
using UnityEngine;
using VoxelMap;

namespace GamePlay.MapFeatures
{
    public class MapDestruction : IMapFeature, IDamageVisitor
    {
        private readonly MapProvider _mapProvider;
        private readonly VoxelHealthSystem _voxelHealthSystem;
        private readonly ColumnDestructionAlgorithm _destructionAlgorithm;

        public MapDestruction(MapProvider mapProvider, VoxelHealthSystem voxelHealthSystem = null,
            ColumnDestructionAlgorithm destructionAlgorithm = null)
        {
            _mapProvider = mapProvider;
            _voxelHealthSystem = voxelHealthSystem;
            _destructionAlgorithm = destructionAlgorithm;
        }

        public void Visit(RangeWeaponData rangeWeapon, RaycastHit hit)
        {
            var position = Vector3Int.FloorToInt(hit.point - hit.normal / 2);
            var voxel = new Voxel(position, _mapProvider.GetVoxelByGlobalPosition(position));
            if (IsDestructible(voxel))
            {
                var constantDamageCalculator = new ConstantDamageCalculator(rangeWeapon.Damage);
                var voxels = new List<Voxel>(1) { voxel };
                HandleVoxels(voxels, constantDamageCalculator);
            }
        }

        public void Visit(MeleeWeaponData meleeWeapon, bool isStrongHit, RaycastHit hit)
        {
            var hitPosition = Vector3Int.FloorToInt(hit.point - hit.normal / 2);
            var constantDamageCalculator = new ConstantDamageCalculator(meleeWeapon.DamageToBlock);
            List<Voxel> voxels = new List<Voxel>();
            if (isStrongHit)
            {
                const int length = 3;
                for (var i = -length / 2; i <= length / 2; i++)
                {
                    var offset = new Vector3Int(0, i, 0);
                    var blockPosition = hitPosition + offset;
                    var voxel = new Voxel(blockPosition, _mapProvider.GetVoxelByGlobalPosition(blockPosition));
                    if (IsDestructible(voxel))
                    {
                        voxels.Add(voxel);
                    }
                }
            }
            else
            {
                var centeredBlock = new Voxel(hitPosition, _mapProvider.GetVoxelByGlobalPosition(hitPosition));
                voxels.Add(centeredBlock);
            }

            HandleVoxels(voxels, constantDamageCalculator);
        }

        public void Visit(Vector3 center, ExplosionData explosionData)
        {
            var explosionCenter = Vector3Int.FloorToInt(center);
            if (!_mapProvider.IsInsideMap(explosionCenter.x, explosionCenter.y, explosionCenter.z))
            {
                return;
            }

            var voxels = new List<Voxel>();
            for (var x = -explosionData.radius; x <= explosionData.radius; x++)
            {
                for (var y = -explosionData.radius; y <= explosionData.radius; y++)
                {
                    for (var z = -explosionData.radius; z <= explosionData.radius; z++)
                    {
                        var offset = new Vector3Int(x, y, z);
                        var blockPosition = explosionCenter + offset;
                        if (!_mapProvider.IsInsideMap(explosionCenter.x + x, explosionCenter.y + y, explosionCenter.z + z))
                        {
                            continue;
                        }

                        var blockData = _mapProvider.GetVoxelByGlobalPosition(blockPosition);
                        var block = new Voxel(blockPosition, blockData);
                        if (IsDestructible(block) && Vector3.Distance(explosionCenter, explosionCenter) <= explosionData.radius)
                        {
                            voxels.Add(block);
                        }
                    }
                }
            }

            var explosionCenterBlock = new Voxel(explosionCenter,
                _mapProvider.GetVoxelByGlobalPosition(explosionCenter));
            var sphereDamageCalculator = new SphereDamageCalculator(explosionCenterBlock, explosionData.radius, explosionData.damage);
            HandleVoxels(voxels, sphereDamageCalculator);
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
            }

            if (damagedVoxels.Count == 0)
            {
                return;
            }

            if (_destructionAlgorithm != null)
            {
                var destroyedVoxels = damagedVoxels.Where(damagedVoxel => !damagedVoxel.Data.IsSolid()).ToList();
                var fallingVoxels = _destructionAlgorithm.Remove(destroyedVoxels);
                var result = new List<Voxel>();
                result.AddRange(damagedVoxels);
                result.AddRange(fallingVoxels);
                _mapProvider.SetVoxelsByGlobalPositions(result);
            }
            else
            {
                _mapProvider.SetVoxelsByGlobalPositions(damagedVoxels);
            }
        }

        private bool IsDestructible(Voxel voxel)
        {
            return voxel.Data.IsSolid() && voxel.Position.x >= 0 && voxel.Position.x < _mapProvider.Width &&
                   voxel.Position.y > 0 && voxel.Position.y < _mapProvider.Height &&
                   voxel.Position.z >= 0 && voxel.Position.z < _mapProvider.Depth;
        }
    }
}