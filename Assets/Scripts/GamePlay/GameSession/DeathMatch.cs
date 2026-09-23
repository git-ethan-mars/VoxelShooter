using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using R3;
using UnityEngine;
using VoxelMap;
using Networking;
using Networking.Core;
using Networking.Messages;
using Random = UnityEngine.Random;

namespace GamePlay
{
	public class DeathMatch : GameMode
	{
		private const int VotingDuration = 20;
		private const int VotingMaps = 4;
		public readonly DeathmatchScoreboard Scoreboard = new DeathmatchScoreboard();

		public readonly Voting MapVoting;

		private readonly IEntityFactory _entityFactory;
		private readonly SpawnPointService _spawnPointService;
		private readonly LootBoxSpawner _lootBoxSpawner;
		private readonly RespawnService _respawnService;

		private readonly Subject<Unit> _characterDied = new Subject<Unit>();
		private readonly ReactiveProperty<TimeSpan> _timeLeft = new ReactiveProperty<TimeSpan>();

		private readonly Dictionary<NetworkConnectionToClient, DeathMatchPlayerSession> _sessions = new Dictionary<NetworkConnectionToClient, DeathMatchPlayerSession>();

		private GameSettings _gameSettings;

		public Observable<TimeSpan> TimeLeft => _timeLeft;
		public Observable<Unit> CharacterDied => _characterDied;


		public DeathMatch(VSNetworkManager networkManager, EntityContainer entityContainer, IEntityFactory entityFactory, MapProvider mapProvider, LootBoxSpawner lootBoxSpawner, SpawnPointService spawnPointService,
			RespawnService respawnService)
		{
			NetworkManager = networkManager;
			EntityContainer = entityContainer;
			MapProvider = mapProvider;
			MapVoting = Voting.Create(networkManager);
			_entityFactory = entityFactory;
			_spawnPointService = spawnPointService;
			_lootBoxSpawner = lootBoxSpawner;
			_respawnService = respawnService;
		}

		public override void Update()
		{
			base.Update();

			if (MutableGameState.Value != GamePlay.GameState.Playing)
			{
				return;
			}

			_timeLeft.Value -= TimeSpan.FromSeconds(Time.deltaTime);

			if (_timeLeft.Value < TimeSpan.Zero)
			{
				_timeLeft.Value = TimeSpan.Zero;

				if (NetworkManager.mode == NetworkManagerMode.Host)
				{
					CleanUp();

					StartMapVotingFlowAsync().Forget();
				}
			}

			if (NetworkManager.mode == NetworkManagerMode.Host)
			{
				_respawnService.OnUpdate(_sessions, Time.deltaTime);
				_lootBoxSpawner.OnUpdate(Time.deltaTime);
			}
		}

		public override async UniTask StartAsync(GameSettings gameSettings)
		{
			await base.StartAsync(gameSettings);

			_gameSettings = gameSettings;
			_timeLeft.Value = gameSettings.GameDuration;

			_respawnService.SetRespawnTime(gameSettings.RespawnTime);
			_lootBoxSpawner.SetBoxRespawnTime(gameSettings.BoxRespawnTime);

			NetworkManager.MessageReceived
				.OfMessageType<GameSettingsRequest>()
				.Subscribe(directedMessage => OnGameSettingsRequest(directedMessage.Connection))
				.AddTo(NetworkManager);
			NetworkManager.MessageReceived
				.OfMessageType<ChangeGameClassRequest>()
				.Subscribe(directedMessage => OnChangeGameClassRequest(directedMessage.Connection, directedMessage.Message))
				.AddTo(NetworkManager);
			NetworkManager.MessageReceived
				.OfMessageType<CharacterDiedResponse>()
				.Subscribe(_ => OnCharacterDiedResponse())
				.AddTo(NetworkManager);
		}

		public void ChangeClass(GameClass chosenClass)
		{
			NetworkManager.SendRequest(new ChangeGameClassRequest(chosenClass));
		}

		public override async UniTask LoadMapAsync(string mapName)
		{
			MutableGameState.Value = GamePlay.GameState.Loading;

			await base.LoadMapAsync(mapName);
			_spawnPointService.CreateSpawnPoints();

			MutableGameState.Value = GamePlay.GameState.Playing;

			foreach (NetworkConnectionToClient connection in _sessions.Keys)
			{
				Vector3 spawnPosition = _spawnPointService.GetRandomSpawnPoint();
				Spectator spectator = _entityFactory.CreateSpectator(spawnPosition);
				NetworkServer.AddPlayerForConnection(connection, spectator.gameObject);
			}
		}

		protected override void OnAddPlayer(NetworkConnectionToClient connection, string nickName, Texture2D avatar)
		{
			DeathMatchPlayerData playerData = Scoreboard.AddPlayer(connection, nickName, avatar);
			var session = new DeathMatchPlayerSession(connection, playerData);
			_sessions[connection] = session;

			if (MutableGameState.Value == GamePlay.GameState.MapVoting)
			{
				MapVoting.OnAddPlayer(connection);

				foreach ((string mapName, int votes) in MapVoting.VoteByCandidate)
				{
					NetworkManager.SendResponse(connection, new MapVoteUpdateResponse(mapName, votes));
				}
			}

			if (MutableGameState.Value == GamePlay.GameState.Playing)
			{
				Vector3 spawnPosition = _spawnPointService.GetRandomSpawnPoint();
				Spectator spectator = _entityFactory.CreateSpectator(spawnPosition);
				NetworkServer.AddPlayerForConnection(connection, spectator.gameObject);
			}
		}

		protected override void OnRemovePlayer(NetworkConnectionToClient connection)
		{
			Scoreboard.RemovePlayer(connection);
			_sessions.Remove(connection);
		}

		protected override void CleanUp()
		{
			base.CleanUp();

			_lootBoxSpawner.Clean();

			foreach (DeathMatchPlayerSession session in _sessions.Values)
			{
				session.IsAlive = false;
				session.RespawnTime = TimeSpan.Zero;
			}
		}

		private async UniTaskVoid StartMapVotingFlowAsync()
		{
			MutableGameState.Value = GamePlay.GameState.MapVoting;
			_timeLeft.Value = GameSettings.GameDuration;

			const string votingTitle = "Map Voting";
			string[] mapNames = MapDataReader.GetExistedMaps().ToArray();
			string[] mapCandidates = new string[Math.Min(VotingMaps, mapNames.Length)];

			for (int i = 0; i < mapCandidates.Length; i++)
			{
				int candidateIndex = Random.Range(i, mapNames.Length);
				(mapNames[i], mapNames[candidateIndex]) = (mapNames[candidateIndex], mapNames[i]);
				mapCandidates[i] = mapNames[i];
			}

			string mapName = await MapVoting.RunVotingAsync(TimeSpan.FromSeconds(VotingDuration), votingTitle, mapCandidates,
				NetworkManager.destroyCancellationToken);

			await LoadMapAsync(mapName);
			NetworkManager.SendResponseToAll(new MapChangeResponse());
		}

		private void OnGameSettingsRequest(NetworkConnectionToClient connection)
		{
			var gameSettings = new GameSettings(MapProvider.Map.CurrentValue.MapName, _timeLeft.Value, _gameSettings.RespawnTime,
				_gameSettings.BoxRespawnTime);
			var response = new GameSettingsResponse(gameSettings);
			connection.Send(response);
		}

		private void OnChangeGameClassRequest(NetworkConnectionToClient connection, ChangeGameClassRequest request)
		{
			if (!_sessions.TryGetValue(connection, out DeathMatchPlayerSession session))
			{
				Debug.LogWarning($"Couldn't find session for id: {connection.connectionId}");
				return;
			}

			if (session.Data.GameClass == request.GameClass)
			{
				return;
			}

			if (session.Data.GameClass == GameClass.None)
			{
				session.Data = session.Data.WithGameClass(request.GameClass);
				_respawnService.Respawn(session);
			}
			else if (session.IsAlive)
			{
				session.Data = session.Data.WithGameClass(request.GameClass);
				_respawnService.Kill(session);
			}

			Scoreboard.ChangeClass(connection, request.GameClass);
		}

		private void OnCharacterDiedResponse()
		{
			_characterDied.OnNext(Unit.Default);
		}
	}
}
