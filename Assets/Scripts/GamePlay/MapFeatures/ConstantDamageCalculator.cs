using VoxelMap;
namespace GamePlay
{
	public class ConstantDamageCalculator : IVoxelDamageCalculator
	{
		private readonly int _damage;

		public ConstantDamageCalculator(int damage)
		{
			_damage = damage;
		}

		public int CalculateDamage(Voxel voxel)
		{
			return _damage;
		}
	}
}