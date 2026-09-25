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
		// Length of the map voting music track (Audio/Sounds/golosovanie-ks.mp3).
		private const float VotingDuration = 18.05f;
		private const int VotingMaps = 4;
		public readonly DeathmatchScoreboard Scoreboard = new DeathmatchScoreboard();

		public readonly Voting MapVoting;
		public readonly Chat Chat;

		private readonly IEntityFactory _entityFactory;
		private readonly SpawnPointService _spawnPointService;
		private readonly LootBoxSpawner _lootBoxSpawner;
		private readonly RespawnService _respawnService;
		private readonly KillList _killList;

		private readonly Subject<Unit> _characterDied = new Subject<Unit>();
		private readonly ReactiveProperty<TimeSpan> _timeLeft = new ReactiveProperty<TimeSpan>();

		private readonly Dictionary<NetworkConnectionToClient, DeathMatchPlayerSession> _sessions = new Dictionary<NetworkConnectionToClient, DeathMatchPlayerSession>();

		private GameSettings _gameSettings;

		public Observable<TimeSpan> TimeLeft => _timeLeft;
		public Observable<Unit> CharacterDied => _characterDied;


		public DeathMatch(VSNetworkManager networkManager, EntityContainer entityContainer, IEntityFactory entityFactory, MapProvider mapProvider, LootBoxSpawner lootBoxSpawner, SpawnPointService spawnPointService,
			RespawnService respawnService, KillList killList)
		{
			NetworkManager = networkManager;
			EntityContainer = entityContainer;
			MapProvider = mapProvider;
			MapVoting = Voting.Create(networkManager);
			Chat = Chat.Create(networkManager, GetNickName);
			_entityFactory = entityFactory;
			_spawnPointService = spawnPointService;
			_lootBoxSpawner = lootBoxSpawner;
			_respawnService = respawnService;
			_killList = killList;

			RegisterChatCommands();
		}

		public override void Update()
		{
			base.Update();

			if (MutableGameState.Value != GamePlay.GameState.Playing)
			{
				return;
			}

			_timeLeft.Value -= TimeSpan.FromSeconds(Time.deltaTime);

			if (NetworkManager.mode != NetworkManagerMode.Host)
			{
				return;
			}

			if (_timeLeft.Value < TimeSpan.Zero)
			{
				_timeLeft.Value = TimeSpan.Zero;
				{
					CleanUp();
					StartMapVotingFlowAsync().Forget();
				}
			}
			else
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
			_killList.KillAdded
				.Subscribe(OnKillAdded)
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
		}

		protected override void OnAddPlayer(NetworkConnectionToClient connection, string nickName, Texture2D avatar)
		{
			DeathMatchPlayerData playerData = Scoreboard.AddPlayer(connection, nickName, avatar);
			var session = new DeathMatchPlayerSession(connection, playerData);
			_sessions[connection] = session;
			Chat.SendSystemMessage($"{nickName} joined the game");

			if (MutableGameState.Value == GamePlay.GameState.MapVoting)
			{
				MapVoting.OnAddPlayer(connection);

				foreach ((string mapName, int votes) in MapVoting.VoteByCandidate)
				{
					NetworkManager.SendResponse(connection, new MapVoteUpdateResponse(mapName, votes));
				}
			}
		}

		protected override void OnPlayerReady(NetworkConnectionToClient connection)
		{
			TrySpawnSpectator(connection);
		}

		protected override void OnRemovePlayer(NetworkConnectionToClient connection)
		{
			string nickName = GetNickName(connection);

			Scoreboard.RemovePlayer(connection);
			_sessions.Remove(connection);

			if (nickName != null)
			{
				Chat.SendSystemMessage($"{nickName} left the game");
			}
		}

		protected override void CleanUp()
		{
			base.CleanUp();

			_lootBoxSpawner.Clean();
			_spawnPointService.Clear();

			foreach (DeathMatchPlayerSession session in _sessions.Values)
			{
				session.IsAlive = false;
				session.RespawnTime = TimeSpan.Zero;
				session.Data = session.Data.WithGameClass(GameClass.None);
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

			SetRemoteClientsNotReady();
			NetworkManager.SendResponseToAll(new MapChangeResponse());

			SetClientReady();
		}

		private void TrySpawnSpectator(NetworkConnectionToClient connection)
		{
			if (MutableGameState.Value != GamePlay.GameState.Playing || connection.identity != null ||
			    !_sessions.ContainsKey(connection))
			{
				return;
			}

			Vector3 spawnPosition = _spawnPointService.GetRandomSpawnPoint();
			Spectator spectator = _entityFactory.CreateSpectator(spawnPosition);
			NetworkServer.AddPlayerForConnection(connection, spectator.gameObject);
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
			if (MutableGameState.Value != GamePlay.GameState.Playing)
			{
				return;
			}

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

		private void OnKillAdded(KillData kill)
		{
			if (!NetworkServer.active)
			{
				return;
			}

			DeathMatchPlayerSession victim = FindSession(kill.TargetId);

			if (victim == null)
			{
				return;
			}

			DeathMatchPlayerSession killer = kill.SourceId == kill.TargetId ? null : FindSession(kill.SourceId);
			_respawnService.Kill(victim, killer?.Connection);
		}

		private void RegisterChatCommands()
		{
			Chat.RegisterCommand(new ChatCommand("players", "/players", "List the connected players with their ids", _ => ListPlayers()));
			Chat.RegisterCommand(new ChatCommand("kick", "/kick <id>", "Disconnect a player from the game", KickPlayer));
			Chat.RegisterCommand(new ChatCommand("kill", "/kill <id>", "Kill a player's character", KillPlayer));
			Chat.RegisterCommand(new ChatCommand("endround", "/endround", "Finish the round and start the map voting", _ => EndRound()));
		}

		private string ListPlayers()
		{
			return string.Join("\n", _sessions.Values.Select(session =>
				$"#{session.Connection.connectionId} {session.Data.NickName} ({session.Data.GameClass})"));
		}

		private string KickPlayer(string[] arguments)
		{
			if (!TryFindSession(arguments, out DeathMatchPlayerSession session, out string error))
			{
				return error ?? "Usage: /kick <id>";
			}

			if (session.Connection == NetworkServer.localConnection)
			{
				return "You can't kick yourself";
			}

			Chat.SendSystemMessage($"{session.Data.NickName} was kicked by the host");
			session.Connection.Disconnect();
			return null;
		}

		private string KillPlayer(string[] arguments)
		{
			if (!TryFindSession(arguments, out DeathMatchPlayerSession session, out string error))
			{
				return error ?? "Usage: /kill <id>";
			}

			if (MutableGameState.Value != GamePlay.GameState.Playing || !session.IsAlive)
			{
				return $"{session.Data.NickName} is not alive";
			}

			_respawnService.Kill(session);
			Chat.SendSystemMessage($"{session.Data.NickName} was killed by the host");
			return null;
		}

		// Finds a player by the id shown in /players. The error is null when the id is missing.
		private bool TryFindSession(string[] arguments, out DeathMatchPlayerSession session, out string error)
		{
			session = null;
			error = null;

			if (arguments.Length == 0)
			{
				return false;
			}

			if (!int.TryParse(arguments[0].TrimStart('#'), out int connectionId))
			{
				error = $"{arguments[0]} is not a player id, see /players";
				return false;
			}

			session = _sessions.Values.FirstOrDefault(candidate => candidate.Connection.connectionId == connectionId);

			if (session == null)
			{
				error = $"Player #{connectionId} not found, see /players";
				return false;
			}

			return true;
		}

		private string EndRound()
		{
			if (MutableGameState.Value != GamePlay.GameState.Playing)
			{
				return "The round is not running";
			}

			// The next update sees the timer run out and starts the map voting.
			_timeLeft.Value = TimeSpan.Zero;
			Chat.SendSystemMessage("The host ended the round");
			return null;
		}

		private string GetNickName(NetworkConnectionToClient connection)
		{
			return _sessions.TryGetValue(connection, out DeathMatchPlayerSession session) ? session.Data.NickName : null;
		}

		private DeathMatchPlayerSession FindSession(PlayerId playerId)
		{
			return _sessions.Values.FirstOrDefault(session => new PlayerId(session.Connection.connectionId) == playerId);
		}

		private void OnCharacterDiedResponse()
		{
			_characterDied.OnNext(Unit.Default);
		}
	}
}
