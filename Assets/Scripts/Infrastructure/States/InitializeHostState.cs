using System;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay;
using GamePlay.MapFeatures;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;
using Services;
using UI;
using UnityEngine;
using VoxelMap;
using Object = UnityEngine.Object;
namespace Infrastructure.States
{
	public class InitializeHostState : IPayloadedState<WorldSettings>
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly IMapFactory _mapFactory;
		private readonly IAssetProvider _assets;
		private readonly IUIFactory _uiFactory;
		private readonly IStaticDataService _staticData;
		private readonly ISpawnPointService _spawnPointService;
		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly MapProvider _mapProvider;
		private readonly EntityContainerService _entityContainer;
		private readonly LootBoxDropper _lootBoxDropper;
		private readonly GameClassChanger _gameClassChanger;
		private readonly IPlayerDataLoader _playerDataLoader;
		private readonly IPlayerService _playerService;
		private readonly MapSender _mapSender;

		private LoadingWindow _loadingWindow;

		public InitializeHostState(GameStateMachine gameStateMachine, IMapFactory mapFactory, IUIFactory uiFactory, IStaticDataService staticData,
			ISpawnPointService spawnPointService, VoxelShooterNetworkManager networkManager, MapProvider mapProvider, EntityContainerService
				entityContainer, LootBoxDropper lootBoxDropper, GameClassChanger gameClassChanger, MapSender mapSender, IPlayerDataLoader playerDataLoader,
			IPlayerService playerService)
		{
			_gameStateMachine = gameStateMachine;
			_mapFactory = mapFactory;
			_uiFactory = uiFactory;
			_staticData = staticData;
			_spawnPointService = spawnPointService;
			_networkManager = networkManager;
			_mapProvider = mapProvider;
			_entityContainer = entityContainer;
			_lootBoxDropper = lootBoxDropper;
			_gameClassChanger = gameClassChanger;
			_mapSender = mapSender;
			_playerDataLoader = playerDataLoader;
			_playerService = playerService;
		}

		public async void Enter(WorldSettings worldSettings)
		{
			_loadingWindow = _uiFactory.CreateLoadingWindow();

			_mapProvider.MapName = worldSettings.MapName;
			_mapProvider.Confgure = worldSettings.Configure;
			_mapProvider.Map = await CreateMap(worldSettings);
			_spawnPointService.CreateSpawnPoints();
			
			_networkManager.StartHost();
			await _networkManager.MessageReceived.OfMessageType<AuthenticationResponse>().FirstAsync().AsUniTask();
			_networkManager.authenticator.OnClientAuthenticated.Invoke();
			
			_lootBoxDropper.StartDropping(worldSettings.BoxSpawnTime, _networkManager.HostStopped).Forget();
			_gameClassChanger.Initialize(worldSettings);
			_mapSender.Initialize();

			_gameStateMachine.Enter<GameLoopState>();
		}

		public void Exit()
		{
			Object.Destroy(_loadingWindow.gameObject);
		}

		private void OnServerAuthenticated()
		{
			
		}

		private async UniTask<Map> CreateMap(WorldSettings worldSettings)
		{
			MapData mapData = await MapDataReader.ReadFromFileAsync(worldSettings.MapName);
			MapBuilder mapBuilder = new MapBuilder(_mapFactory, mapData).FromConfigure(worldSettings.Configure);
			var mapBuildProgress = new Progress<float>();
			mapBuildProgress.ProgressChanged += OnMapLoadingProgressed;
			Map map = await mapBuilder.BuildAsync(mapBuildProgress);
			mapBuildProgress.ProgressChanged -= OnMapLoadingProgressed;
			var voxelHealthSystem = new VoxelHealthSystem(map, _staticData);
			var mapBuilding = new MapBuilding(_entityContainer, map, voxelHealthSystem);
			map.AddMapFeature(mapBuilding);
			var mapDestruction = new MapDestruction(map, voxelHealthSystem);
			map.AddMapFeature(mapDestruction);
			return map;
		}

		private void OnMapLoadingProgressed(object sender, float value)
		{
			_loadingWindow.UpdateLoadingBar(value);
		}
	}
}