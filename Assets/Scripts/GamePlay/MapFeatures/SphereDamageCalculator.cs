using UnityEngine;
using VoxelMap;
namespace GamePlay.MapFeatures
{
	public class SphereDamageCalculator : IVoxelDamageCalculator
	{
		private readonly Vector3Ushort _center;
		private readonly int _damage;
		private readonly int _radius;

		public SphereDamageCalculator(Vector3Ushort center, int radius, int damage)
		{
			_center = center;
			_radius = radius;
			_damage = damage;
		}

		public int CalculateDamage(Voxel voxel)
		{
			return (int)((1 - Vector3.Distance(voxel.Position, _center) / _radius) * _damage);
		}
	}
}