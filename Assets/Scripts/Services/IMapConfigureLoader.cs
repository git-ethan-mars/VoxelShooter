using Data;
using VoxelMap.Data;

namespace Services
{
	public interface IMapConfigureLoader
	{
		MapConfigure GetMapConfigure(string mapName);
	}
}