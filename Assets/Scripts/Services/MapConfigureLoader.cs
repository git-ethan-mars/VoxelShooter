using System.Collections.Generic;
using System.Linq;
using Data;
using VoxelMap.Data;

namespace Services
{
	public class MapConfigureLoader : IMapConfigureLoader
	{
		private const string Default = "Default";
		private const string MapConfiguresPath = "StaticData/Map configures";

		private readonly Dictionary<string, MapConfigure> _mapConfigureByName;

		public MapConfigureLoader(IAssetProvider assets)
		{
			_mapConfigureByName = assets.LoadAll<MapConfigure>(MapConfiguresPath)
				.ToDictionary(configure => configure.name, configure => configure);
		}

		public MapConfigure GetMapConfigure(string mapName)
		{
			return _mapConfigureByName.TryGetValue(mapName, out MapConfigure mapConfigure)
				? mapConfigure
				: GetDefaultMapConfigure();
		}

		private MapConfigure GetDefaultMapConfigure()
		{
			return _mapConfigureByName[Default];
		}
	}
}