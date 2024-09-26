using VoxelMap;

namespace GamePlay.Destruction
{
	public interface IBlockDamageCalculator
	{
		int CalculateDamage(BlockDataWithPosition block);
	}
}