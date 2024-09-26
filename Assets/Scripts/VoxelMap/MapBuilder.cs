using Common.AssetManagement;
using Common.StaticData;
using UnityEngine;

namespace VoxelMap
{
	public class MapBuilder
	{
		private readonly IMapFactory _mapFactory;

		private Color32 _waterColor = BlockData.Air.Color;
		private bool _waterColorChanging;
		private Color32 _innerColor = BlockData.Inner.Color;
		private bool _innerColorChanging;
		private bool _fogActivating;
		private bool _enableWalls;
		private FogData _fog;
		private LightData _light;
		private Material _skybox;
		private AmbientData _ambient;

		public MapBuilder(IAssetProvider assets)
		{
			_mapFactory = new MapFactory(assets);
		}

		public MapBuilder WithWaterColor(Color32 color)
		{
			_waterColorChanging = true;
			_waterColor = color;
			return this;
		}

		public MapBuilder WithInnerColor(Color32 color)
		{
			_innerColorChanging = true;
			_innerColor = color;
			return this;
		}

		public MapBuilder WithFog(FogData fogData)
		{
			_fogActivating = true;
			_fog = fogData;
			return this;
		}

		public MapBuilder WithDirectionalLight(LightData lightData)
		{
			_light = lightData;
			return this;
		}

		public MapBuilder WithWalls()
		{
			_enableWalls = true;
			return this;
		}

		public MapBuilder WithSkybox(Material skybox)
		{
			_skybox = skybox;
			return this;
		}

		public MapBuilder WithAmbient(AmbientData ambient)
		{
			_ambient = ambient;
			return this;
		}

		public Map Build(MapData mapData, Transform container = null)
		{
			var map = _mapFactory.CreateMap(mapData, container);
			var mapGenerator = new MapGenerator(_mapFactory, mapData);
			var chunks = mapGenerator.GenerateChunks(map.transform);
			map.SetChunks(chunks);

			if (_light is not null)
			{
				var directionalLight = _mapFactory.CreateDirectionalLight(_light, map.transform);
				map.SetDirectionalLight(directionalLight);
			}

			if (_waterColorChanging)
			{
				map.SetWaterColor(_waterColor);
				var mapCenter = new Vector3((float) mapData.Width / 2, 1, (float) mapData.Depth / 2);
				var waterPlane = _mapFactory.CreateWaterPlane(mapCenter, _waterColor, map.transform);
				map.SetWaterLayer(waterPlane);
			}
			
			if (_innerColorChanging)
			{
				map.SetInnerColor(_innerColor);
			}
			
			if (_enableWalls)
			{
				var walls = _mapFactory.CreateWalls(mapData, map.transform);
				map.SetWalls(walls);
			}
			
			if (_fogActivating)
			{
				map.SetFog(_fog);
			}
			
			if (_skybox)
			{
				map.SetSkybox(_skybox);
			}
			
			if (_ambient != null)
			{
				map.SetAmbient(_ambient);
			}

			return map;
		}
	}
}