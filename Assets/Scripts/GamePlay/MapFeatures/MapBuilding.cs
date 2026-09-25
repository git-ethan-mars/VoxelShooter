using System.Collections.Generic;
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

		public bool Visit(Blueprint blueprint, RaycastHit rayCastHit, Color32 color)
		{
			Vector3Int blueprintPosition = blueprint.GetPlacementPosition(rayCastHit);
			using PooledObject<List<Vector3Int>> pooledList = ListPool<Vector3Int>.Get(out List<Vector3Int> positions);

			foreach (Vector3Int offset in blueprint.Positions)
			{
				positions.Add(blueprintPosition + offset);
			}

			return Build(positions, color);
		}

		public bool Build(IReadOnlyList<Vector3Int> positions, Color32 color)
		{
			using PooledObject<List<Voxel>> pooledList = ListPool<Voxel>.Get(out List<Voxel> voxels);

			foreach (Vector3Int position in positions)
			{
				if (!IsAvailablePosition(position))
				{
					return false;
				}

				voxels.Add(new Voxel((Vector3Ushort)position, new VoxelData(color)));
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
