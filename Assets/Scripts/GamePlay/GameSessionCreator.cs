using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking;
using Networking.Audio;
using Networking.Core;
using Networking.Messages;
using R3;
using Services;
using VoxelMap;
namespace GamePlay
{
	public class GameSessionCreator
	{
		private readonly IMapFactory _mapFactory;
		private readonly IMapConfigureLoader _mapConfigureLoader;
		private readonly VSNetworkManager _networkManager;
		private readonly MapProvider _mapProvider;
		private readonly IPlayerService _playerService;
		private readonly IEntityFactory _entityFactory;
		private readonly GameStateDownloader _gameDownloader;
		private readonly GameStateSender _gameStateSender;
		private readonly LootBoxDropper _lootBoxDropper;
		private readonly EntityContainerService _entityContainer;
		private readonly ISpawnPointService _spawnPointService;
		private readonly NetworkAudioPlayer _audioPlayer;

		public GameSessionCreator(EntityContainerService entityContainer, IMapFactory mapFactory,
			IMapConfigureLoader mapConfigureLoader, VSNetworkManager networkManager, GameStateDownloader gameDownloader,
			LootBoxDropper lootBoxDropper, ISpawnPointService spawnPointService, MapProvider mapProvider,
			IPlayerService playerService, IEntityFactory entityFactory, NetworkAudioPlayer audioPlayer)
		{
			_entityContainer = entityContainer;
			_mapFactory = mapFactory;
			_mapConfigureLoader = mapConfigureLoader;
			_networkManager = networkManager;
			_gameDownloader = gameDownloader;
			_gameStateSender = new GameStateSender(networkManager, mapProvider);
			_lootBoxDropper = lootBoxDropper;
			_spawnPointService = spawnPointService;
			_mapProvider = mapProvider;
			_playerService = playerService;
			_entityFactory = entityFactory;
			_audioPlayer = audioPlayer;
		}

		public GameSession Create(GameSettings gameSettings)
		{
			_networkManager.StartHost();
			
			var gameSession = new GameSession(gameSettings, _mapProvider, _networkManager, _mapFactory, _mapConfigureLoader, _entityFactory,
				_lootBoxDropper, _entityContainer, _spawnPointService, _playerService);

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
				.OfMessageType<GameSettingsRequest>()
				.Subscribe(directedMessage => _gameStateSender.SendGameSettings(directedMessage.Connection, gameSession.GameSettings))
				.AddTo(_networkManager);
			_networkManager.MessageReceived
				.OfMessageType<ChangeClassRequest>()
				.Subscribe(directedMessage => gameSession.ChangeClass(directedMessage.Connection, directedMessage.Message.GameClass).Forget())
				.AddTo(_networkManager);
			_networkManager.MessageReceived.OfMessageType<StaticAudioResponse>()
				.Subscribe(directedMessage => _audioPlayer.Play(directedMessage.Message.AudioType, directedMessage.Message.Position).Forget())
				.AddTo(_networkManager);
			_networkManager.MessageReceived.OfMessageType<DynamicAudioResponse>()
				.Subscribe(directedMessage => _audioPlayer.Play(directedMessage.Message.AudioType, directedMessage.Message.NetworkIdentity,
					directedMessage.Message.IsSpatial).Forget())
				.AddTo(_networkManager);

			return gameSession;
		}

		public async UniTask<GameSession> ConnectToGameSession()
		{
			_networkManager.StartClient();
			await _networkManager.MessageReceived.FirstAsync<AuthenticationResponse>();
			NetworkClient.connection.isAuthenticated = true;

			_networkManager.MessageReceived.OfMessageType<StaticAudioResponse>()
				.Subscribe(directedMessage => _audioPlayer.Play(directedMessage.Message.AudioType, directedMessage.Message.Position).Forget())
				.AddTo(_networkManager);
			_networkManager.MessageReceived.OfMessageType<DynamicAudioResponse>()
				.Subscribe(directedMessage => _audioPlayer.Play(directedMessage.Message.AudioType, directedMessage.Message.NetworkIdentity,
					directedMessage.Message.IsSpatial).Forget())
				.AddTo(_networkManager);

			_gameDownloader.DownloadMapUpdatesAsync(_networkManager.destroyCancellationToken).Forget();

			var gameSession = new GameSession(_mapProvider, _networkManager, _gameDownloader);

			return gameSession;
		}
	}
}