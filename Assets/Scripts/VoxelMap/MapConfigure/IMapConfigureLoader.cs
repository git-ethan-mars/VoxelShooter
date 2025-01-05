using Common;

namespace VoxelMap
{
	public interface IMapConfigureLoader : IService
	{
		MapConfigure GetMapConfigure(string mapName);
	}
}