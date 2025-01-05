using UnityEngine;
using VoxelMap;

namespace GamePlay.MapFeatures
{
	public class SphereDamageCalculator : IVoxelDamageCalculator
	{
		private readonly Voxel _center;
		private readonly int _radius;
		private readonly int _damage;

		public SphereDamageCalculator(Voxel center, int radius, int damage)
		{
			_center = center;
			_radius = radius;
			_damage = damage;
		}
		
		public int CalculateDamage(Voxel voxel)
		{
			return (int) ((1 - Vector3Int.Distance(voxel.Position, _center.Position) / _radius) * _damage);
		}
	}
}