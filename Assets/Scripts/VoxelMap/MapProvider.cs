using System;
using Cysharp.Threading.Tasks;
using R3;
using Services;
using UnityEngine;
using VoxelMap.Data;
using Object = UnityEngine.Object;

namespace VoxelMap
{
	public class MapProvider
	{
		private readonly IMapConfigureLoader _mapConfigLoader;
		private readonly ReactiveProperty<Map> _map = new ReactiveProperty<Map>();
		private readonly ReactiveProperty<MapConfigure> _loadingMapConfigure = new ReactiveProperty<MapConfigure>();

		public ReadOnlyReactiveProperty<Map> Map => _map;
		public ReadOnlyReactiveProperty<MapConfigure> LoadingMapConfigure => _loadingMapConfigure;
		public IProgress<float> MapLoadingProgress { get; set; }

		public MapProvider(IMapConfigureLoader mapConfigLoader)
		{
			_mapConfigLoader = mapConfigLoader;
		}

		// Announces the map before its data is read or downloaded, so the loading screen can show it right away.
		public void BeginLoading(string mapName)
		{
			_loadingMapConfigure.Value = _mapConfigLoader.GetMapConfigure(mapName);
		}

		public async UniTask LoadMapAsync(MapData mapData, string mapName)
		{
			if (_map.Value != null)
			{
				Object.Destroy(_map.Value.gameObject);
			}

			MapConfigure mapConfigure = _mapConfigLoader.GetMapConfigure(mapName);
			MapBuilder mapBuilder = new MapBuilder().FromConfigure(mapConfigure);
			_map.Value = await mapBuilder.BuildAsync(mapData, mapName, MapLoadingProgress, Application.exitCancellationToken);
		}
	}
}
