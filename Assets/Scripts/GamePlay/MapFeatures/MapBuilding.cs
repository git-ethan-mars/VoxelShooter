using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;
namespace GamePlay
{
	public class MapBuilding : MapFeature, IBuildVisitor
	{
		private MapProvider _mapProvider;
		private EntityContainer _entityContainer;

		[Inject]
		private void Construct(MapProvider mapProvider, EntityContainer entityContainer)
		{
			_mapProvider = mapProvider;
			_entityContainer = entityContainer;
		}

		public bool Visit(Block block, RaycastHit rayCastHit, Color32 color)
		{
			Vector3Ushort voxelPosition = Vector3Ushort.FloorToUshort(rayCastHit.point + rayCastHit.normal / 2);
			var voxel = new Voxel(voxelPosition, new VoxelData(color));

			using var pooledList = ListPool<Voxel>.Get(out var voxels);
			voxels.Add(voxel);

			if (!IsAvailablePosition(voxel.Position))
			{
				return false;
			}

			_mapProvider.Map.CurrentValue.SetVoxelsByGlobalPositions(voxels);

			return true;
		}

		public bool Visit(Blueprint blueprint, RaycastHit rayCastHit, Color32 color)
		{
			Vector3Int blueprintPosition = blueprint.GetPlacementPosition(rayCastHit);
			using var pooledList = ListPool<Voxel>.Get(out var voxels);

			foreach (Vector3Int offset in blueprint.Configure.Positions)
			{
				Vector3Int voxelPosition = blueprintPosition + offset;

				if (!IsAvailablePosition(voxelPosition))
				{
					return false;
				}

				voxels.Add(new Voxel((Vector3Ushort)voxelPosition, new VoxelData(color)));
			}

			_mapProvider.Map.CurrentValue.SetVoxelsByGlobalPositions(voxels);
			return true;
		}

		public bool IsAvailablePosition(Vector3Int position)
		{
			if (!_mapProvider.Map.CurrentValue.IsInsideMap(position))
			{
				return false;
			}

			if (_mapProvider.Map.CurrentValue.MapData[(ushort)position.x, (ushort)position.y, (ushort)position.z].IsSolid())
			{
				return false;
			}

			foreach (Entity entity in _entityContainer.GetEntitiesByType<Entity>())
			{
				if (entity.Bounds.min.x < position.x + 1 && entity.Bounds.max.x > position.x &&
				    entity.Bounds.min.y < position.y + 1 && entity.Bounds.max.y > position.y &&
				    entity.Bounds.min.z < position.z + 1 && entity.Bounds.max.z > position.z)
				{
					return false;
				}
			}

			return true;
		}
	}
}