using VoxelMap;

namespace GamePlay.Destruction
{
	public class ConstantDamageCalculator : IBlockDamageCalculator
	{
		private readonly int _damage;

		public ConstantDamageCalculator(int damage)
		{
			_damage = damage;
		}
		
		public int CalculateDamage(BlockDataWithPosition block)
		{
			return _damage;
		}
	}
}