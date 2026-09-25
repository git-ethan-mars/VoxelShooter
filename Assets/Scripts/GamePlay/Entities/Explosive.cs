using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;
using EntityId = Data.EntityId;

namespace GamePlay
{
	public abstract class Explosive : Entity
	{
		protected MapProvider MapProvider;
		public abstract ExplosiveType Type { get; }

		public PlayerId? OwnerId
		{
			get
			{
				if (netIdentity.connectionToClient is null)
				{
					return null;
				}

				return new PlayerId(netIdentity.connectionToClient.connectionId);
			}
		}

		protected void Explode(ExplosionData explosionData)
		{
			if (MapProvider.Map.CurrentValue.TryGetFeature(out MapDestruction mapDestruction))
			{
				var explosionCenter = Vector3Ushort.FloorToUshort(transform.position);

				if (!MapProvider.Map.CurrentValue.IsInsideMap(explosionCenter.x, explosionCenter.y, explosionCenter.z))
				{
					return;
				}

				ListPool<Voxel>.Get(out List<Voxel> voxels);
				var damageCalculator = new SphereDamageCalculator(explosionCenter, explosionData.radius, explosionData.damage);

				for (ushort x = (ushort)Mathf.Max(explosionCenter.x - explosionData.radius, 0);
				     x <= Mathf.Min(explosionCenter.x + explosionData.radius,
					     MapProvider.Map.CurrentValue.Width);
				     x++)
				{
					for (ushort y = (ushort)Mathf.Max(explosionCenter.y - explosionData.radius, 0);
					     y <= Mathf.Min(explosionCenter.y + explosionData.radius, MapProvider.Map.CurrentValue.Height);
					     y++)
					{
						for (ushort z = (ushort)Mathf.Max(explosionCenter.z - explosionData.radius, 0);
						     z <= Mathf.Min(explosionCenter.z + explosionData.radius, MapProvider.Map.CurrentValue.Depth);
						     z++)
						{
							var position = new Vector3Ushort(x, y, z);

							VoxelData blockData = MapProvider.Map.CurrentValue.GetVoxelByGlobalPosition(position);
							var voxel = new Voxel(position, blockData);

							if (mapDestruction.IsDestructible(voxel) && Vector3.Distance(explosionCenter, position) <
							    explosionData.radius)
							{
								voxels.Add(voxel);
							}
						}
					}
				}

				mapDestruction.HandleVoxels(voxels, damageCalculator);
				ListPool<Voxel>.Release(voxels);
			}

			foreach (IDamageVisitor visitor in EntityContainer.GetEntitiesByType<IDamageVisitor>().ToList())
			{
				visitor.Visit(this, explosionData);
			}
		}
	}
}
