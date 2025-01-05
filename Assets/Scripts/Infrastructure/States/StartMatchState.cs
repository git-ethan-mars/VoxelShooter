using Common.AssetManagement;
using GamePlay.Data;
using GamePlay.MapFeatures;
using Infrastructure.PlayerDataLoader;
using Networking;
using Networking.Client;
using VoxelMap;

namespace Infrastructure.States
{
	public class StartMatchState : IPayloadedState<WorldSettings>
	{
		private readonly SceneLoader _sceneLoader;
		private readonly IAssetProvider _assets;
		private readonly INetworkingFactory _networkingFactory;
		private readonly IMapConfigureLoader _mapConfigureLoader;
		private readonly IMeshFactory _meshFactory;
		private readonly GameStateMachine _stateMachine;
		private IAuthenticationService _authenticationService;
		private const string Main = "Main";

		public StartMatchState(GameStateMachine stateMachine,
			SceneLoader sceneLoader, IAssetProvider assets, INetworkingFactory networkingFactory, IMapConfigureLoader mapConfigureLoader)
		{
			_stateMachine = stateMachine;
			_sceneLoader = sceneLoader;
			_assets = assets;
			_networkingFactory = networkingFactory;
			_mapConfigureLoader = mapConfigureLoader;
		}

		public void Enter(WorldSettings worldSettings)
		{
			_sceneLoader.Load(Main, () => CreateHost(worldSettings));
		}

		private async void CreateHost(WorldSettings worldSettings)
		{
			var mapData = MapReader.ReadFromFile(worldSettings.MapName);
			var mapConfigure = _mapConfigureLoader.GetMapConfigure(worldSettings.MapName);
			var mapBuilder = new MapBuilder(_assets)
				.WithWaterColor(mapConfigure.WaterColor)
				.WithInnerColor(mapConfigure.InnerColor)
				.WithAmbient(mapConfigure.AmbientData)
				.WithDirectionalLight(mapConfigure.LightData)
				.WithFog(mapConfigure.FogData)
				.WithSkybox(mapConfigure.SkyboxMaterial)
				.WithWalls();

			var map = await mapBuilder.BuildAsync(mapData);
			var mapProvider = new MapProvider(map);
			var host = _networkingFactory.CreateHost(worldSettings, mapProvider);
			host.Start();
			_stateMachine.Enter<GameLoopState, (IClient, MapProvider)>((host, mapProvider));
		}
		
		public void Exit()
		{
		}
	}
}