using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay.MapFeatures;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;
using Services;
using UnityEngine;
using VoxelMap;
using Object = UnityEngine.Object;
namespace GamePlay
{
	public class GameSession : IDisposable
	{
		public CancellationToken MapChangeToken => _mapChangeCancellationTokenSource.Token;
		public ReadOnlyReactiveProperty<GameSessionState> State => _state;
		public ReadOnlyReactiveProperty<TimeSpan> TimeLeft => _timeLeft;

		private readonly MapProvider _mapProvider;
		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly GameStateDownloader _gameDownloader;
		private readonly IMapFactory _mapFactory;
		private readonly IMapConfigureLoader _mapConfigureLoader;
		private readonly LootBoxDropper _lootBoxDropper;
		private readonly EntityContainerService _entityContainer;
		private readonly ISpawnPointService _spawnPointService;
		private readonly IPlayerService _playerService;
		private readonly IStaticDataService _staticData;
		private readonly GameSettings _gameSettings;
		private readonly ReactiveProperty<TimeSpan> _timeLeft = new ReactiveProperty<TimeSpan>();
		private readonly ReactiveProperty<GameSessionState> _state = new ReactiveProperty<GameSessionState>();
		private bool _isRunning;
		private CancellationTokenSource _cts = new CancellationTokenSource();
		private CancellationTokenSource _mapChangeCancellationTokenSource = new CancellationTokenSource();


		public GameSession(GameSettings gameSettings, MapProvider mapProvider, VoxelShooterNetworkManager networkManager,
			IMapFactory mapFactory, IMapConfigureLoader mapConfigureLoader, LootBoxDropper lootBoxDropper, EntityContainerService entityContainer,
			ISpawnPointService spawnPointService, IPlayerService playerService, IStaticDataService staticData)
		{
			_gameSettings = gameSettings;
			_mapProvider = mapProvider;
			_networkManager = networkManager;
			_mapFactory = mapFactory;
			_mapConfigureLoader = mapConfigureLoader;
			_lootBoxDropper = lootBoxDropper;
			_entityContainer = entityContainer;
			_spawnPointService = spawnPointService;
			_playerService = playerService;
			_staticData = staticData;
		}

		public GameSession(MapProvider mapProvider, VoxelShooterNetworkManager networkManager, GameStateDownloader gameDownloader)
		{
			_mapProvider = mapProvider;
			_networkManager = networkManager;
			_gameDownloader = gameDownloader;
		}

		public async UniTask RunAsync(IProgress<float> progress)
		{
			if (_isRunning)
			{
				return;
			}

			_isRunning = true;
			_state.Value = GameSessionState.Waiting;

			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				bool isCanceled = await ChangeMapAsync(_gameSettings.MapName, progress).SuppressCancellationThrow();

				if (isCanceled)
				{
					return;
				}

				StartGame();
			}
			if (_networkManager.mode == NetworkManagerMode.ClientOnly)
			{
				_networkManager.MessageReceived
					.OfMessageType<MapChangeResponse>()
					.Subscribe(_ => OnMapChangedMessage(progress))
					.AddTo(_networkManager);

				bool isCanceled = await DownloadGameStateAsync(progress, MapChangeToken).SuppressCancellationThrow();

				if (!isCanceled)
				{
					StartGame();
				}
				else
				{
					Debug.Log("Download game state cancelled");
				}
			}

			_timeLeft
				.Where(time => time <= TimeSpan.Zero && _state.Value == GameSessionState.Playing)
				.Subscribe(_ => OnTimeFinished(progress))
				.AddTo(_networkManager);
		}

		public void Dispose()
		{
			_cts?.Dispose();
			_mapChangeCancellationTokenSource?.Dispose();

			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				_networkManager.StopHost();
			}
			else
			{
				_networkManager.StopClient();
			}
		}

		private void StartGame()
		{
			_state.Value = GameSessionState.Playing;

			_cts = new CancellationTokenSource();

			Observable.Interval(TimeSpan.FromSeconds(1), _cts.Token)
				.Subscribe(_ =>
				{
					_timeLeft.Value -= TimeSpan.FromSeconds(1);
				})
				.AddTo(_cts.Token);

			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				_lootBoxDropper.StartDropping(_gameSettings.BoxSpawnTime, _cts.Token).Forget();
			}
		}

		private async UniTask ChangeMapAsync(string mapName, IProgress<float> progress)
		{
			if (_networkManager.mode != NetworkManagerMode.Host)
			{
				return;
			}

			if (mapName == _mapProvider.MapName)
			{
				Debug.LogWarning("Can't change map to the same one");
				return;
			}

			MapData mapData = await MapDataReader.ReadFromFileAsync(mapName, MapChangeToken);
			MapConfigure mapConfigure = _mapConfigureLoader.GetMapConfigure(mapName);
			MapBuilder mapBuilder = new MapBuilder(_mapFactory, mapData).FromConfigure(mapConfigure);
			Map map = await mapBuilder.BuildAsync(progress, Application.exitCancellationToken);

			var voxelHealthSystem = new VoxelHealthSystem(map, _staticData);
			var mapBuilding = new MapBuilding(_entityContainer, map, voxelHealthSystem);
			map.AddMapFeature(mapBuilding);
			var mapDestruction = new MapDestruction(map, voxelHealthSystem);
			map.AddMapFeature(mapDestruction);

			_mapProvider.MapName = mapName;
			_mapProvider.Map = map;

			_spawnPointService.CreateSpawnPoints();

			_timeLeft.Value = TimeSpan.FromSeconds(30);
		}

		private async UniTask DownloadGameStateAsync(IProgress<float> progress, CancellationToken cancellationToken = default)
		{
			string mapName = await _gameDownloader.DownloadMapNameAsync(cancellationToken);
			Map map = await _gameDownloader.DownloadMapAsync(mapName, progress, cancellationToken);
			TimeSpan timeLeft = await _gameDownloader.DownloadGameTime(cancellationToken);
			_mapProvider.MapName = mapName;
			_mapProvider.Map = map;
			_timeLeft.Value = timeLeft;

			NetworkClient.Ready();
		}

		private async void OnTimeFinished(IProgress<float> progress)
		{
			_cts.Cancel();
			_cts.Dispose();

			_state.Value = GameSessionState.Waiting;

			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				Object.Destroy(_mapProvider.Map.gameObject);

				foreach (Entity entity in _entityContainer.GetEntitiesByType<Entity>())
				{
					NetworkServer.Destroy(entity.gameObject);
				}

				_mapChangeCancellationTokenSource.Cancel();
				_mapChangeCancellationTokenSource.Dispose();
				_mapChangeCancellationTokenSource = new CancellationTokenSource();

				foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
				{
					if (connection != NetworkServer.localConnection)
					{
						NetworkServer.SetClientNotReady(connection);
					}
				}

				_playerService.ResetData();

				string mapName = await PickRandomMapName();
				bool isCanceled = await ChangeMapAsync(mapName, progress).SuppressCancellationThrow();

				if (isCanceled)
				{
					return;
				}

				_networkManager.SendResponseToAll(new MapChangeResponse());
				Debug.Log("Change map message send");

				StartGame();
			}
		}

		private async void OnMapChangedMessage(IProgress<float> progress)
		{
			Debug.Log("Map change message recieved");

			_mapChangeCancellationTokenSource.Cancel();
			_mapChangeCancellationTokenSource.Dispose();
			_mapChangeCancellationTokenSource = new CancellationTokenSource();

			if (_mapProvider.Map != null)
			{
				Object.Destroy(_mapProvider.Map.gameObject);
			}

			bool isCanceled = await DownloadGameStateAsync(progress, MapChangeToken).SuppressCancellationThrow();

			if (!isCanceled)
			{
				StartGame();
			}
			else
			{
				Debug.Log("Download game state cancelled");
			}
		}

		private async UniTask<string> PickRandomMapName()
		{
			string[] mapNames = MapDataReader.GetExistedMaps().ToArray();
			string mapName = mapNames[UnityEngine.Random.Range(0, mapNames.Length)];

			while (_mapProvider.MapName == mapName)
			{
				mapName = mapNames[UnityEngine.Random.Range(0, mapNames.Length)];
				await UniTask.Yield();
			}

			return mapName;
		}
	}
}