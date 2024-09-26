using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Common;
using Common.Factory;
using Common.StaticData;
using Cysharp.Threading.Tasks;
using Entities;
using Entities.PlayerLogic;
using Infrastructure.Factory;
using Mirror;
using Networking.Client;
using Networking.Host.Services;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;
using VoxelMap;
using MemoryStream = System.IO.MemoryStream;

namespace Networking.Host
{
	public sealed partial class MirrorHost : IHost
	{
		public CancellationToken OnHostStopped { get; private set; }

		public IClient InnerClient { get; }

		public event Action<Character> PlayerDied;

		public MapUpdater MapUpdater { get; }

		private readonly string _mapName;
		private readonly MapProvider _mapProvider;
		private readonly NetworkManager _networkManager;
		private readonly IStaticDataService _staticData;
		private readonly WorldSettings _worldSettings;
		private readonly IEntityFactory _entityFactory;
		private readonly EntityPositionValidator _entityPositionValidator;
		private readonly BoxDropper _boxDropper;
		private readonly SpawnPointService _spawnPointService;
		private readonly HostTimer _hostTimer;
		private readonly FallDamage _fallDamage;
		private readonly Dictionary<ulong, Player> _playerDataById = new();
		private readonly Dictionary<NetworkConnectionToClient, ulong> _idByConnection = new();
		private readonly MeleeWeaponValidator _meleeWeaponValidator;
		private readonly RocketLauncherValidator _rocketLauncherValidator;
		private readonly DrillValidator _drillValidator;

		public MirrorHost(IClient client, NetworkManager networkManager,
			IStaticDataService staticData,
			IParticleFactory particleFactory, IEntityFactory entityFactory, WorldSettings worldSettings)
		{
			InnerClient = client;
			_networkManager = networkManager;
			_staticData = staticData;
			_entityFactory = entityFactory;
			_worldSettings = worldSettings;
			_mapProvider = new MapProvider(MapReader.ReadFromFile(worldSettings.MapName));
			_hostTimer = new HostTimer(this, worldSettings.MaxDuration);
			MapUpdater = new MapUpdater(staticData, _mapProvider);
			_entityPositionValidator = new EntityPositionValidator(MapUpdater);
			_boxDropper = new BoxDropper(this, worldSettings, _mapProvider, entityFactory);
			_fallDamage = new FallDamage(this, staticData);
			_meleeWeaponValidator = new MeleeWeaponValidator(this, coroutineRunner, particleFactory, MapUpdater);
			_rocketLauncherValidator = new RocketLauncherValidator(this, coroutineRunner, entityFactory);
			_drillValidator = new DrillValidator(this, coroutineRunner, entityFactory);
		}

		public void ChangeClass(GameClass gameClass)
		{
			InnerClient.ChangeClass(gameClass);
		}

		public void Start()
		{
			_networkManager.StartHost();

			_hostTimer.Start();
			_entityPositionValidator.Start();
			_boxDropper.Start();
			_fallDamage.Start();

			RegisterMessageHandlers();

			InnerClient.Start();
		}

		public void Stop()
		{
			_networkManager.StopHost();
			_entityPositionValidator.Stop();
			_boxDropper.Stop();
			_fallDamage.Stop();
			UnregisterHandlers();

			InnerClient.Stop();
		}

		public bool AddPlayer(NetworkConnectionToClient connection, ulong id, string nickName)
		{
			if (!_playerDataById.ContainsKey(id))
			{
				_idByConnection[connection] = id;
				_playerDataById[id] = new Player(nickName);
				return true;
			}

			return _idByConnection.TryAdd(connection, id);
		}


		public void SendMap(NetworkConnectionToClient connection)
		{
			connection.Send(new MapNameResponse(_mapName));
			using var memoryStream = new MemoryStream();
			MapWriter.WriteMap(_mapProvider.MapData, memoryStream);
			var bytes = memoryStream.ToArray();
			var mapMessages = MessageSplitter.SplitBytesIntoMessages(bytes, Constants.MessageSize);
			MessageSplitter.SendMessages(mapMessages, Constants.MessageDelay, false, connection);
		}

		public void RemovePlayer(NetworkConnectionToClient connection)
		{
			_idByConnection.Remove(connection);
			NetworkServer.SendToReady(new ScoreboardResponse(GetScoreData()));
			NetworkServer.DestroyPlayerForConnection(connection);
		}

		private void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
		{
			var playerData = GetPlayerData(connection);
			if (playerData.GameClass == GameClass.None)
			{
				playerData.ChangeClass(_staticData, chosenClass);
				var player = _entityFactory.CreateCharacter(_spawnPointService.GetSpawnPosition());
				NetworkServer.AddPlayerForConnection(connection, player.gameObject);
				NetworkServer.SendToReady(new ScoreboardResponse(GetScoreData()));
			}
			else
			{
				playerData.GameClass = chosenClass;
				if (!playerData.IsAlive)
				{
					return;
				}

				Kill(connection);
			}
		}

		public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
		{
			var result = TryGetPlayerData(receiver, out var playerData);
			if (!result || !playerData.IsAlive) return;
			playerData.Health -= totalDamage;
			if (playerData.Health <= 0)
			{
				PlayerDied?.Invoke(receiver.identity.GetComponent<Character>());
				playerData.Health = 0;
				AddKill(source, receiver);
				Kill(receiver);
			}
			else
			{
				receiver.Send(new HealthResponse(playerData.Health));
			}
		}

		public void Heal(NetworkConnectionToClient receiver, int totalHeal)
		{
			var result = TryGetPlayerData(receiver, out var playerData);
			if (!result || !playerData.IsAlive)
			{
				return;
			}

			playerData.Health += totalHeal;
			if (playerData.Health >= playerData.Characteristic.maxHealth)
			{
				playerData.Health = playerData.Characteristic.maxHealth;
			}

			receiver.Send(new HealthResponse(playerData.Health));
		}

		public bool TryGetPlayerData(NetworkConnectionToClient connection, out Player player)
		{
			if (connection is null)
			{
				player = null;
				return false;
			}
			if (_idByConnection.TryGetValue(connection, out var id))
			{
				return _playerDataById.TryGetValue(id, out player);
			}

			player = null;
			return false;
		}

		public Player GetPlayerData(NetworkConnectionToClient connection)
		{
			var id = _idByConnection[connection];
			return _playerDataById[id];
		}

		public void SpawnEntity(Entity entity, NetworkConnectionToClient owner = null)
		{
			NetworkServer.Spawn(entity.gameObject, owner);
		}

		public void UnSpawnEntity(Entity entity)
		{
			if (entity != null)
			{
				NetworkServer.UnSpawn(entity.gameObject);
			}
		}

		public void SpawnParticles(ParticleSystem particles)
		{
			NetworkServer.Spawn(particles.gameObject);
		}

		public void SendAudio(AudioData audio, NetworkIdentity source)
		{
			NetworkServer.SendToReady(new PlayerSoundResponse(_staticData.GetAudioIndex(audio), source));
		}

		public void StartContinuousAudio(AudioData audio, NetworkIdentity source)
		{
			NetworkServer.SendToReady(new StartContinuousSoundResponse(_staticData.GetAudioIndex(audio), source));
		}

		public void StopContinuousSound(NetworkIdentity source)
		{
			NetworkServer.SendToReady(new StopContinuousSoundResponse(source));
		}

		public void StartMuzzleFlash(NetworkIdentity source)
		{
			NetworkServer.SendToReady(new StartMuzzleFlashResponse(source));
		}

		public void StopMuzzleFlash(NetworkIdentity source)
		{
			NetworkServer.SendToReady(new StopMuzzleFlashResponse(source));
		}

		private void RespawnPlayer(NetworkConnectionToClient connection)
		{
			var result = TryGetPlayerData(connection, out var playerData);
			if (!result)
			{
				return;
			}

			playerData.ChangeClass(_staticData, playerData.GameClass);
			var player = _entityFactory.CreateCharacter(_spawnPointService.GetSpawnPosition());
			player.SetNickName(playerData.NickName);
			player.SetCharacteristic(playerData.Characteristic);
			ReplacePlayer(connection, player.gameObject);
		}

		private void ReplacePlayer(NetworkConnectionToClient connection, GameObject newPlayer)
		{
			// TODO : REFACTOR
			if (connection.identity == null)
			{
				return;
			}

			var oldPlayer = connection.identity.gameObject;
			NetworkServer.ReplacePlayerForConnection(connection, newPlayer, true);
			DestroyPlayerAsync(oldPlayer);
		}

		private async UniTaskVoid DestroyPlayerAsync(GameObject oldPlayer)
		{
			await UniTask.Yield();
			if (oldPlayer != null)
			{
				NetworkServer.Destroy(oldPlayer);
			}
		}

		private void Kill(NetworkConnectionToClient connection)
		{
			var tombstonePosition =
				Vector3Int.FloorToInt(connection.identity.transform.position) + Constants.worldOffset;
			var tombstone = _entityFactory.CreateTombstone(tombstonePosition);
			var playerData = GetPlayerData(connection);
			playerData.Die();
			var spectator = _entityFactory.CreateSpectatorPlayer(tombstonePosition);
			ReplacePlayer(connection, spectator.gameObject);
			var respawnTimer = new RespawnTimer(connection, _worldSettings.SpawnTime,
				() => RespawnPlayer(connection));
			respawnTimer.Start();
			NetworkServer.SendToReady(new ScoreboardResponse(GetScoreData()));
		}

		private void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
		{
			if (killer is not null && killer != victim)
			{
				GetPlayerData(killer).Kills += 1;
			}
		}

		private List<ScoreData> GetScoreData()
		{
			var scoreData = new SortedSet<ScoreData>();
			foreach (var playerData in _idByConnection.Keys.Select(GetPlayerData))
			{
				scoreData.Add(new ScoreData(playerData.ID, playerData.NickName, playerData.Kills,
					playerData.Deaths, playerData.GameClass));
			}

			return scoreData.ToList();
		}

		private void RegisterMessageHandlers()
		{
			NetworkServer.RegisterHandler<AddBlocksRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<ChangeClassRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<ChangeSlotRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<IncrementSlotIndexRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<DecrementSlotIndexRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<GrenadeSpawnRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<TntSpawnRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<ShootRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<CancelShootRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<ReloadRequest>(OnRequestReceived);
			NetworkServer.RegisterHandler<HitRequest>(OnRequestReceived);
		}

		private void UnregisterHandlers()
		{
			NetworkServer.UnregisterHandler<AddBlocksRequest>();
			NetworkServer.UnregisterHandler<ChangeClassRequest>();
			NetworkServer.UnregisterHandler<ChangeSlotRequest>();
			NetworkServer.UnregisterHandler<IncrementSlotIndexRequest>();
			NetworkServer.UnregisterHandler<DecrementSlotIndexRequest>();
			NetworkServer.UnregisterHandler<GrenadeSpawnRequest>();
			NetworkServer.UnregisterHandler<TntSpawnRequest>();
			NetworkServer.UnregisterHandler<ShootRequest>();
			NetworkServer.UnregisterHandler<CancelShootRequest>();
			NetworkServer.UnregisterHandler<ReloadRequest>();
			NetworkServer.UnregisterHandler<HitRequest>();
		}
	}
}