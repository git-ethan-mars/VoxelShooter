using UnityEngine;
using VoxelMap;

namespace GamePlay.Destruction
{
	public class SphereDamageCalculator : IBlockDamageCalculator
	{
		private readonly BlockDataWithPosition _center;
		private readonly int _radius;
		private readonly int _damage;

		public SphereDamageCalculator(BlockDataWithPosition center, int radius, int damage)
		{
			_center = center;
			_radius = radius;
			_damage = damage;
		}
		
		public int CalculateDamage(BlockDataWithPosition block)
		{
			return (int) ((1 - Vector3Int.Distance(block.Position, _center.Position) / _radius) * _damage);
		}
	}
}