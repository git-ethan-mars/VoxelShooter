using VoxelMap;
namespace GamePlay.MapFeatures
{
	public interface IVoxelDamageCalculator
	{
		int CalculateDamage(Voxel voxel);
	}
}