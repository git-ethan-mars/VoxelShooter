using System;
using System.Collections.Generic;
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
using Reflex.Extensions;
using Reflex.Injectors;
using Services;
using UnityEngine;
using VoxelMap;
using VoxelMap.Data;
using Object = UnityEngine.Object;
namespace GamePlay
{
	public class GameSession : IDisposable
	{
		public CancellationToken GameSessionFinished => _gameSessionFinishedCancellationTokenSource.Token;
		public CancellationToken MapChangeToken => _mapChangeCancellationTokenSource.Token;
		public ReadOnlyReactiveProperty<GameSessionState> State => _state;
		public ReadOnlyReactiveProperty<TimeSpan> GameTime => _gameTime;
		public ReadOnlyReactiveProperty<TimeSpan> RespawnTime => _respawnTime;
		public Observable<Map> OnMapReady => _onMapReady;
		public IProgress<float> Progress { set => _progress = value; }
		public GameSettings GameSettings { get; private set; }

		private readonly MapProvider _mapProvider;
		private readonly VSNetworkManager _networkManager;
		private readonly GameStateDownloader _gameDownloader;
		private readonly IMapFactory _mapFactory;
		private readonly IMapConfigureLoader _mapConfigureLoader;
		private readonly IEntityFactory _entityFactory;
		private readonly LootBoxDropper _lootBoxDropper;
		private readonly EntityContainerService _entityContainer;
		private readonly ISpawnPointService _spawnPointService;
		private readonly IPlayerService _playerService;
		private bool _isRunning;
		private readonly ReactiveProperty<TimeSpan> _gameTime = new ReactiveProperty<TimeSpan>();
		private readonly ReactiveProperty<TimeSpan> _respawnTime = new ReactiveProperty<TimeSpan>();
		private readonly ReactiveProperty<GameSessionState> _state = new ReactiveProperty<GameSessionState>();
		private readonly Subject<Map> _onMapReady = new Subject<Map>();
		private CancellationTokenSource _mapChangeCancellationTokenSource = new CancellationTokenSource();
		private readonly CancellationTokenSource _gameSessionFinishedCancellationTokenSource = new CancellationTokenSource();
		private IProgress<float> _progress;

		public GameSession(GameSettings gameSettings, MapProvider mapProvider, VSNetworkManager networkManager,
			IMapFactory mapFactory, IMapConfigureLoader mapConfigureLoader, IEntityFactory entityFactory,
			LootBoxDropper lootBoxDropper, EntityContainerService entityContainer,
			ISpawnPointService spawnPointService, IPlayerService playerService)
		{
			GameSettings = gameSettings;
			_mapProvider = mapProvider;
			_networkManager = networkManager;
			_mapFactory = mapFactory;
			_mapConfigureLoader = mapConfigureLoader;
			_entityFactory = entityFactory;
			_lootBoxDropper = lootBoxDropper;
			_entityContainer = entityContainer;
			_spawnPointService = spawnPointService;
			_playerService = playerService;
		}

		public GameSession(MapProvider mapProvider, VSNetworkManager networkManager, GameStateDownloader gameDownloader)
		{
			_mapProvider = mapProvider;
			_networkManager = networkManager;
			_gameDownloader = gameDownloader;
		}

		public async UniTask RunAsync()
		{
			if (_isRunning)
			{
				return;
			}

			_isRunning = true;
			_state.Value = GameSessionState.Waiting;

			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				bool isCanceled = await ChangeMapAsync(GameSettings.MapName).SuppressCancellationThrow();

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
					.Subscribe(_ => OnMapChangedMessage())
					.AddTo(_networkManager);

				bool isCanceled = await DownloadGameStateAsync(MapChangeToken).SuppressCancellationThrow();

				if (!isCanceled)
				{
					StartGame();
				}
				else
				{
					Debug.Log("Download game state cancelled");
				}
			}
		}

		public async UniTask<GameClass> ChangeGameClassAsync(GameClass chosenClass)
		{
			var request = new ChangeClassRequest(chosenClass);
			_networkManager.SendRequest(request);
			var response = await _networkManager.MessageReceived
				.FirstAsync<ChangeGameClassResponse>(cancellationToken: GameSessionFinished);
			
			if (_respawnTime.Value == TimeSpan.Zero)
			{
				_respawnTime.Value = GameSettings.RespawnTime;
			}
			
			return response.Message.GameClass;
		}

		public async UniTaskVoid ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
		{
			if (!_playerService.TryGetPlayerData(connection.connectionId, out PlayerData playerData))
			{
				throw new KeyNotFoundException($"Couldn't find player {connection.connectionId}");
			}

			if (playerData.GameClass == chosenClass)
			{
				return;
			}

			if (playerData.GameClass == GameClass.None)
			{
				playerData.GameClass = chosenClass;
				Character character = await SpawnCharacterAsync(playerData);
				NetworkServer.AddPlayerForConnection(connection, character.gameObject);
				playerData.IsAlive = true;
			}
			else if (playerData.IsAlive)
			{
				connection.identity.GetComponent<Character>().Damage(int.MaxValue);
				playerData.GameClass = chosenClass;
			}
			else
			{
				playerData.GameClass = chosenClass;
			}
			
			var response = new ChangeGameClassResponse(playerData.GameClass);
			_networkManager.SendResponse(connection, response);
		}

		private async UniTask<Character> SpawnCharacterAsync(PlayerData playerData)
		{
			GameClass chosenClass = playerData.GameClass;
			string nickName = playerData.NickName;
			Vector3 position = await _spawnPointService.GetSpawnPointAsync();
			Character character = _entityFactory.CreateCharacter(position, chosenClass, nickName);
			character.HealthSystem.Health
				.Where(healthValue => healthValue == 0)
				.Subscribe(_ => OnCharacterDied(character))
				.AddTo(character);

			return character;
		}

		private async void OnCharacterDied(Character oldCharacter)
		{
			NetworkConnectionToClient connection = oldCharacter.netIdentity.connectionToClient;

			if (!_playerService.TryGetPlayerData(connection.connectionId, out PlayerData playerData))
			{
				return;
			}

			playerData.IsAlive = false;
			Spectator spectator = _entityFactory.CreateSpectator(oldCharacter.transform.position);
			NetworkServer.ReplacePlayerForConnection(connection, spectator.gameObject, ReplacePlayerOptions.Destroy);
			Tombstone tombStone = _entityFactory.CreateTombstone(oldCharacter.transform.position);
			tombStone.ExplodeWithDelay(GameSettings.RespawnTime - TimeSpan.FromSeconds(1)).Forget();

			TimeSpan respawnTime = TimeSpan.FromSeconds(Time.time) + GameSettings.RespawnTime;

			while (respawnTime > TimeSpan.FromSeconds(Time.time))
			{
				await UniTask.Yield();
			}

			if (connection.isReady)
			{
				Character character = await SpawnCharacterAsync(playerData);
				NetworkServer.ReplacePlayerForConnection(connection, character.gameObject, ReplacePlayerOptions.Destroy);
				playerData.IsAlive = true;
			}
		}

		public void Dispose()
		{
			_gameSessionFinishedCancellationTokenSource.Cancel();
			_gameSessionFinishedCancellationTokenSource.Dispose();
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

			var timerCts = new CancellationTokenSource();
			var endGameTime = TimeSpan.FromSeconds(Time.time) + GameSettings.GameDuration;

			Observable.EveryUpdate(timerCts.Token).Subscribe(_ =>
				{
					var timeLeft = endGameTime - TimeSpan.FromSeconds(Time.time);

					if (timeLeft > TimeSpan.Zero)
					{
						_gameTime.Value = timeLeft;
						
						if (_respawnTime.Value > TimeSpan.Zero)
						{
							_respawnTime.Value -= TimeSpan.FromSeconds(Time.deltaTime);
							
							if (_respawnTime.Value < TimeSpan.Zero)
							{
								_respawnTime.Value = TimeSpan.Zero;
							}
						}
					}
					else
					{
						timerCts.Cancel();
						timerCts.Dispose();
					}
				},
				_ => OnTimerFinished());

			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				_lootBoxDropper.StartDropping(GameSettings.BoxRespawnTime, timerCts.Token).Forget();
			}
		}

		private async UniTask ChangeMapAsync(string mapName)
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
			Map map = await mapBuilder.BuildAsync(_progress, Application.exitCancellationToken);

			map.AddFeature<MapBuilding>();
			map.AddFeature<MapDestruction>();
			map.AddFeature<VoxelHealthSystem>();
			map.AddFeature<MapUpdateSender>();
			//map.AddFeature<ColumnDestructionAlgorithm>();

			_mapProvider.MapName = mapName;
			_mapProvider.Map = map;

			GameObjectInjector.InjectObject(map.gameObject, map.gameObject.scene.GetSceneContainer());

			_spawnPointService.CreateSpawnPoints();

			_onMapReady.OnNext(map);

			_gameTime.Value = GameSettings.GameDuration;
		}

		private async UniTask DownloadGameStateAsync(CancellationToken cancellationToken = default)
		{
			string mapName = await _gameDownloader.DownloadMapNameAsync(cancellationToken);
			Map map = await _gameDownloader.DownloadMapAsync(mapName, _progress, cancellationToken);
			GameSettings gameSettings = await _gameDownloader.DownloadGameSettings(cancellationToken);
			_mapProvider.MapName = mapName;
			_mapProvider.Map = map;
			GameSettings = gameSettings;
			_gameTime.Value = gameSettings.GameDuration;

			_onMapReady.OnNext(map);

			NetworkClient.Ready();
		}

		private async void OnTimerFinished()
		{
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
				bool isCanceled = await ChangeMapAsync(mapName).SuppressCancellationThrow();

				if (isCanceled)
				{
					return;
				}

				_networkManager.SendResponseToAll(new MapChangeResponse());
				Debug.Log("Change map message send");

				StartGame();
			}
		}

		private async void OnMapChangedMessage()
		{
			Debug.Log("Map change message received");

			_mapChangeCancellationTokenSource.Cancel();
			_mapChangeCancellationTokenSource.Dispose();
			_mapChangeCancellationTokenSource = new CancellationTokenSource();

			if (_mapProvider.Map != null)
			{
				Object.Destroy(_mapProvider.Map.gameObject);
			}

			bool isCanceled = await DownloadGameStateAsync(MapChangeToken).SuppressCancellationThrow();

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