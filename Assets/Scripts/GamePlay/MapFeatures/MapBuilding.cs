using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;
namespace GamePlay.MapFeatures
{
	public class MapBuilding : MapFeature, IBuildVisitor
	{
		private MapProvider _mapProvider;
		private EntityContainerService _entityContainer;

		[Inject]
		private void Construct(MapProvider mapProvider, EntityContainerService entityContainer)
		{
			_mapProvider = mapProvider;
			_entityContainer = entityContainer;
		}
		
		public void Visit(Block block, RaycastHit rayCastHit)
		{
			if (block.Amount.Value <= 0)
			{
				return;
			}

			Vector3Ushort voxelPosition = Vector3Ushort.FloorToUshort(rayCastHit.point + rayCastHit.normal / 2);
			var voxel = new Voxel(voxelPosition, new VoxelData(block.Color));
			
			ListPool<Voxel>.Get(out var voxels);
			voxels.Add(voxel);
			
			if (!_mapProvider.Map.IsInsideMap(voxel.Position) || !CanBuild(voxel))
			{
				return;
			}

			_mapProvider.Map.SetVoxelsByGlobalPositions(voxels);
			
			block.Amount.Value -= 1;
			
			ListPool<Voxel>.Release(voxels);
		}

		private bool CanBuild(Voxel voxel)
		{
			foreach (Entity entity in _entityContainer.GetEntitiesByType<Entity>())
			{
				if (entity.Bounds.min.x < voxel.Position.x + 1 && entity.Bounds.max.x > voxel.Position.x &&
				    entity.Bounds.min.y < voxel.Position.y + 1 && entity.Bounds.max.y > voxel.Position.y &&
				    entity.Bounds.min.z < voxel.Position.z + 1 && entity.Bounds.max.z > voxel.Position.z)
				{
					return false;
				}
			}

			return true;
		}
	}
}