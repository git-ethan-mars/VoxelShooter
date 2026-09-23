using Data;
using UnityEngine;
using UnityEngine.Pool;
using VoxelMap;
using EntityId = Data.EntityId;

namespace GamePlay
{
	public abstract class Explosive : Entity
	{
		public abstract ExplosiveType Type { get; }
		
		protected MapProvider MapProvider;
		
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
				Vector3Ushort explosionCenter = Vector3Ushort.FloorToUshort(transform.position);

				if (!MapProvider.Map.CurrentValue.IsInsideMap(explosionCenter.x, explosionCenter.y, explosionCenter.z))
				{
					return;
				}

				ListPool<Voxel>.Get(out var voxels);
				var damageCalculator = new SphereDamageCalculator(explosionCenter, explosionData.radius, explosionData.damage);

				for (var x = (ushort)Mathf.Max(explosionCenter.x - explosionData.radius, 0);
				     x <= Mathf.Min(explosionCenter.x + explosionData.radius,
					     MapProvider.Map.CurrentValue.Width);
				     x++)
				{
					for (var y = (ushort)Mathf.Max(explosionCenter.y - explosionData.radius, 0);
					     y <= Mathf.Min(explosionCenter.y + explosionData.radius, MapProvider.Map.CurrentValue.Height);
					     y++)
					{
						for (var z = (ushort)Mathf.Max(explosionCenter.z - explosionData.radius, 0);
						     z <= Mathf.Min(explosionCenter.z + explosionData.radius, MapProvider.Map.CurrentValue.Depth);
						     z++)
						{
							Vector3Ushort position = new Vector3Ushort(x, y, z);

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

			foreach (IDamageVisitor visitor in EntityContainer.GetEntitiesByType<IDamageVisitor>())
			{
				visitor.Visit(this, explosionData);
			}
		}
	}
}