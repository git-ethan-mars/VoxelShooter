using System.Collections.Generic;
using GamePlay.Data;
using GamePlay.Entities;
using UnityEngine;
using VoxelMap;

namespace GamePlay.MapFeatures
{
    public class MapBuilding : IMapFeature, IBuildVisitor
    {
        private readonly MapProvider _mapProvider;
        private readonly VoxelHealthSystem _voxelHealthSystem;
        private readonly ColumnDestructionAlgorithm _destructionAlgorithm;

        public MapBuilding(MapProvider mapProvider, VoxelHealthSystem voxelHealthSystem = null, ColumnDestructionAlgorithm destructionAlgorithm = null)
        {
            _mapProvider = mapProvider;
            _voxelHealthSystem = voxelHealthSystem;
            _destructionAlgorithm = destructionAlgorithm;
        }

        public void Visit(BlockData blockData, RaycastHit rayCastHit)
        {
            var voxelPosition = Vector3Int.FloorToInt(rayCastHit.point + rayCastHit.normal / 2);
            var voxel = new Voxel(voxelPosition, new VoxelData(blockData.SelectedColor));
            var voxels = new List<Voxel> { voxel };
            if (!CanBuild(voxels))
            {
                return;
            }

            _voxelHealthSystem?.RestoreVoxels(voxels);
            _destructionAlgorithm?.Add(voxels);
            _mapProvider.SetVoxelsByGlobalPositions(voxels);
        }

        private bool CanBuild(List<Voxel> voxels)
        {
            foreach (var character in Entity.GetEntitiesByType<Character>())
            {
                for (var i = 0; i < voxels.Count; i++)
                {
                    if (_mapProvider.GetVoxelByGlobalPosition(voxels[i].Position).IsSolid() &&
                        IsVoxelOverlapCharacter(voxels[i], character))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsVoxelOverlapCharacter(Voxel voxel, Character character)
        {
            var characterPosition = character.transform.position;
            return characterPosition.x > voxel.Position.x
                   && characterPosition.x < voxel.Position.x + 1
                   && characterPosition.z > voxel.Position.z
                   && characterPosition.z < voxel.Position.z + 1
                   && characterPosition.y > voxel.Position.y - 2
                   && characterPosition.y < voxel.Position.y + 2;
        }
    }
}