using Data;

namespace Services
{
	public interface IMapConfigureLoader
	{
		MapConfigure GetMapConfigure(string mapName);
	}
}