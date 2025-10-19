using System.Collections.Generic;
using UnityEngine;
using VoxelMap;
namespace GamePlay.MapFeatures
{
	public class MapBuilding : IMapFeature, IBuildVisitor
	{
		private readonly ColumnDestructionAlgorithm _destructionAlgorithm;
		private readonly EntityContainerService _entityContainerService;
		private readonly Map _map;
		private readonly VoxelHealthSystem _voxelHealthSystem;

		public MapBuilding(EntityContainerService entityContainerService, Map map, VoxelHealthSystem voxelHealthSystem = null,
			ColumnDestructionAlgorithm destructionAlgorithm = null)
		{
			_entityContainerService = entityContainerService;
			_map = map;
			_voxelHealthSystem = voxelHealthSystem;
			_destructionAlgorithm = destructionAlgorithm;
		}

		public void Visit(Block block, RaycastHit rayCastHit)
		{
			if (block.Amount.Value <= 0)
			{
				return;
			}

			Vector3Int voxelPosition = Vector3Int.FloorToInt(rayCastHit.point + rayCastHit.normal / 2);
			var voxel = new Voxel(voxelPosition, new VoxelData(block.SelectedColor));
			var voxels = new List<Voxel> { voxel };
			if (!CanBuild(voxels))
			{
				return;
			}

			_voxelHealthSystem?.RestoreVoxels(voxels);
			_destructionAlgorithm?.Add(voxels);
			_map.SetVoxelsByGlobalPositions(voxels);
			block.Amount.Value -= 1;
		}

		public void OnAdd()
		{
			var buildVisitorWrapper = _map.gameObject.AddComponent<BuildVisitorWrapper>();
			buildVisitorWrapper.Construct(this);
		}

		private bool CanBuild(List<Voxel> voxels)
		{
			foreach (IEntity entity in _entityContainerService.GetEntitiesByType<IEntity>())
			{
				for (var i = 0; i < voxels.Count; i++)
				{
					if (entity.Bounds.min.x < voxels[i].Position.x + 1 && entity.Bounds.max.x > voxels[i].Position.x &&
					    entity.Bounds.min.y < voxels[i].Position.y + 1 && entity.Bounds.max.y > voxels[i].Position.y &&
					    entity.Bounds.min.z < voxels[i].Position.z + 1 && entity.Bounds.max.z > voxels[i].Position.z)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}