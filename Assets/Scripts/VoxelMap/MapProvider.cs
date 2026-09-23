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

		public ReadOnlyReactiveProperty<Map> Map => _map;
		public IProgress<float> MapLoadingProgress { get; set; }

		public MapProvider(IMapConfigureLoader mapConfigLoader)
		{
			_mapConfigLoader = mapConfigLoader;
		}

		public async UniTask LoadMap(MapData mapData, string mapName)
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