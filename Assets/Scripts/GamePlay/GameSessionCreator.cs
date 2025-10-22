using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;
using Services;
using UnityEngine;
using VoxelMap;
namespace GamePlay
{
	public class GameSessionCreator
	{
		private readonly IMapFactory _mapFactory;
		private readonly IMapConfigureLoader _mapConfigureLoader;
		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly MapProvider _mapProvider;
		private readonly IPlayerService _playerService;
		private readonly GameStateDownloader _gameDownloader;
		private readonly GameStateSender _gameStateSender;
		private readonly GameClassChanger _gameClassChanger;
		private readonly LootBoxDropper _lootBoxDropper;
		private readonly EntityContainerService _entityContainer;
		private readonly ISpawnPointService _spawnPointService;
		private readonly IStaticDataService _staticData;

		public GameSessionCreator(EntityContainerService entityContainer, IMapFactory mapFactory,
			IMapConfigureLoader mapConfigureLoader, VoxelShooterNetworkManager networkManager, GameStateDownloader gameDownloader,
			GameClassChanger gameClassChanger, LootBoxDropper lootBoxDropper, ISpawnPointService spawnPointService, MapProvider mapProvider,
			IPlayerService playerService, IStaticDataService staticData)
		{
			_entityContainer = entityContainer;
			_mapFactory = mapFactory;
			_mapConfigureLoader = mapConfigureLoader;
			_networkManager = networkManager;
			_gameDownloader = gameDownloader;
			_gameStateSender = new GameStateSender(networkManager, mapProvider);
			_gameClassChanger = gameClassChanger;
			_lootBoxDropper = lootBoxDropper;
			_spawnPointService = spawnPointService;
			_mapProvider = mapProvider;
			_playerService = playerService;
			_staticData = staticData;
		}

		public GameSession Create(GameSettings gameSettings)
		{
			_networkManager.StartHost();
			_gameClassChanger.Initialize(gameSettings);

			var gameSession = new GameSession(gameSettings, _mapProvider, _networkManager, _mapFactory, _mapConfigureLoader, _lootBoxDropper,
				_entityContainer, _spawnPointService, _playerService, _staticData);

			_networkManager.MessageReceived
				.OfMessageType<MapNameRequest>()
				.Subscribe(directedMessage => _gameStateSender.SendMapName(directedMessage.Connection))
				.AddTo(_networkManager);
			_networkManager.MessageReceived
				.OfMessageType<MapDownloadRequest>()
				.Subscribe(directedMessage =>
					_gameStateSender.SendMapAsync(directedMessage.Connection, gameSession.MapChangeToken).Forget())
				.AddTo(_networkManager);
			_networkManager.MessageReceived
				.OfMessageType<GameTimeRequest>()
				.Subscribe(directedMessage => _gameStateSender.SendGameTime(directedMessage.Connection, gameSession.TimeLeft.CurrentValue))
				.AddTo(_networkManager);
			Observable.EveryUpdate()
				.Where(_ => Input.GetKeyDown(KeyCode.X))
				.Subscribe(_ =>
				{
					_gameStateSender.SendMapAsync(NetworkServer.localConnection, gameSession.MapChangeToken).Forget();
				})
				.AddTo(_networkManager);
			_networkManager.MessageReceived
				.OfMessageType<ChangeClassRequest>()
				.Subscribe(directedMessage => _gameClassChanger.ChangeClass(directedMessage.Connection, directedMessage.Message.GameClass))
				.AddTo(_networkManager);

			return gameSession;
		}

		public async UniTask<GameSession> ConnectToGameSession()
		{
			_networkManager.StartClient();
			await _networkManager.MessageReceived.FirstAsync<AuthenticationResponse>();
			NetworkClient.connection.isAuthenticated = true;

			var gameSession = new GameSession(_mapProvider, _networkManager, _gameDownloader);

			return gameSession;
		}
	}
}