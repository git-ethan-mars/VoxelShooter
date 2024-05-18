using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data;
using Entities;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using MapLogic;
using Mirror;
using Networking.MessageHandlers.RequestHandlers;
using Networking.Messages.Responses;
using Networking.ServerServices;
using PlayerLogic.States;
using Steamworks;
using UnityEngine;

namespace Networking
{
    public class Server : IServer
    {
        public MapProvider MapProvider { get; }
        public MapUpdater MapUpdater { get; }
        public EntityContainer EntityContainer { get; }
        public IEnumerable<NetworkConnectionToClient> ClientConnections => _dataByConnection.Keys;
        public BlockHealthSystem BlockHealthSystem { get; }

        private readonly ServerSettings _serverSettings;
        private readonly IPlayerFactory _playerFactory;
        private readonly IEntityFactory _entityFactory;
        private readonly IStaticDataService _staticData;
        private readonly EntityPositionValidator _entityPositionValidator;
        private readonly BoxDropService _boxDropService;
        private readonly SpawnPointService _spawnPointService;
        private readonly ServerTimer _serverTimer;
        private readonly CustomNetworkManager _networkManager;
        private readonly AddBlocksHandler _addBlocksHandler;
        private readonly ChangeClassHandler _changeClassHandler;
        private readonly GrenadeSpawnHandler _grenadeSpawnHandler;
        private readonly TntSpawnHandler _tntSpawnHandler;
        private readonly ChangeSlotHandler _changeSlotHandler;
        private readonly IncrementSlotIndexHandler _incrementSlotIndexHandler;
        private readonly DecrementSlotIndexHandler _decrementSlotIndexHandler;
        private readonly ShootHandler _shootHandler;
        private readonly CancelShootHandler _cancelShootHandler;
        private readonly ReloadHandler _reloadHandler;
        private readonly HitHandler _hitHandler;
        private readonly AuthenticationHandler _authenticationHandler;
        private readonly FallDamageService _fallDamageService;
        private readonly Dictionary<NetworkConnectionToClient, PlayerData> _dataByConnection;

        public Server(CustomNetworkManager networkManager, ServerSettings serverSettings)
        {
            _networkManager = networkManager;
            _entityFactory = networkManager.EntityFactory;
            _playerFactory = networkManager.PlayerFactory;
            _staticData = networkManager.StaticData;
            _serverSettings = serverSettings;
            MapProvider = MapReader.ReadFromFile(serverSettings.MapName, networkManager.StaticData);
            MapUpdater = new MapUpdater(networkManager, MapProvider);
            EntityContainer = new EntityContainer();
            _entityPositionValidator = new EntityPositionValidator(this);
            _spawnPointService =
                new SpawnPointService(this, networkManager.GameFactory, networkManager.EntityFactory);
            _serverTimer = new ServerTimer(networkManager, serverSettings.MaxDuration);
            _boxDropService = new BoxDropService(this, networkManager, serverSettings);
            BlockHealthSystem = new BlockHealthSystem(networkManager.StaticData, this);
            _fallDamageService = new FallDamageService(this, networkManager);
            var audioService = new AudioService(networkManager.StaticData);
            var muzzleFlashService = new MuzzleFlashService();
            var rangeWeaponValidator = new RangeWeaponValidator(this, networkManager, audioService, muzzleFlashService);
            var meleeWeaponValidator = new MeleeWeaponValidator(this, networkManager, audioService);
            var rocketLauncherValidator = new RocketLauncherValidator(this, networkManager, audioService);
            var drillValidator = new DrillValidator(this, networkManager, audioService);
            _addBlocksHandler = new AddBlocksHandler(this);
            _changeClassHandler = new ChangeClassHandler(this);
            _changeSlotHandler = new ChangeSlotHandler(this, audioService);
            _incrementSlotIndexHandler = new IncrementSlotIndexHandler(this, audioService);
            _decrementSlotIndexHandler = new DecrementSlotIndexHandler(this, audioService);
            _grenadeSpawnHandler = new GrenadeSpawnHandler(this, networkManager, audioService);
            _tntSpawnHandler =
                new TntSpawnHandler(this, networkManager, audioService);
            _shootHandler = new ShootHandler(this, rangeWeaponValidator, rocketLauncherValidator, drillValidator);
            _cancelShootHandler = new CancelShootHandler(this, rangeWeaponValidator);
            _reloadHandler = new ReloadHandler(this, rangeWeaponValidator, rocketLauncherValidator, drillValidator);
            _hitHandler = new HitHandler(this, meleeWeaponValidator);
            _authenticationHandler = new AuthenticationHandler(this);
            _dataByConnection = new Dictionary<NetworkConnectionToClient, PlayerData>();
        }

        public void Start()
        {
            RegisterHandlers();
            _serverTimer.Start();
            _entityPositionValidator.Start();
            _boxDropService.Start();
            _spawnPointService.CreateSpawnPoints();
            _fallDamageService.Start();
        }

        public void AddPlayer(NetworkConnectionToClient connection, CSteamID steamID,
            string nickname)
        {
            var playerData = new PlayerData(steamID, nickname, _staticData);
            _dataByConnection[connection] = playerData;
        }

        public void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
        {
            var playerData = GetPlayerData(connection);
            if (playerData.GameClass == GameClass.None)
            {
                playerData.GameClass = chosenClass;
                playerData.PlayerStateMachine.Enter<LifeState>();
                var player = _playerFactory.CreatePlayer(_spawnPointService.GetSpawnPosition());
                NetworkServer.AddPlayerForConnection(connection, player);
                connection.Send(new PlayerConfigureResponse(playerData.Characteristic.placeDistance,
                    playerData.Characteristic.speed, playerData.Characteristic.jumpHeight,
                    playerData.Items.Select(item => item.id).ToList(),
                    playerData.Health));
                SendDataFromOtherPlayers(connection);
                NetworkServer.SendToReady(new NickNameResponse(connection.identity, playerData.NickName));
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

        private void Kill(NetworkConnectionToClient connection)
        {
            var tombstonePosition =
                Vector3Int.FloorToInt(connection.identity.transform.position) + Constants.worldOffset;
            var tombstone = _entityFactory.CreateTombstone(tombstonePosition, this, connection);
            EntityContainer.AddPushable(tombstone.GetComponent<IPushable>());
            var playerData = GetPlayerData(connection);
            playerData.PlayerStateMachine.Enter<DeathState>();
            var spectator = _playerFactory.CreateSpectatorPlayer(tombstonePosition);
            ReplacePlayer(connection, spectator);
            var respawnTimer = new RespawnTimer(_networkManager, connection, _serverSettings.SpawnTime,
                () => RespawnPlayer(connection));
            respawnTimer.Start();
            connection.Send(new SpectatorConfigureResponse());
            NetworkServer.SendToReady(new ScoreboardResponse(GetScoreData()));
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            _dataByConnection.Remove(connection);
            NetworkServer.SendToReady(new ScoreboardResponse(GetScoreData()));
            NetworkServer.DestroyPlayerForConnection(connection);
        }

        public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
        {
            var result = TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive) return;
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
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
            if (!result || !playerData.IsAlive) return;
            playerData.Health += totalHeal;
            if (playerData.Health >= playerData.Characteristic.maxHealth)
            {
                playerData.Health = playerData.Characteristic.maxHealth;
            }

            receiver.Send(new HealthResponse(playerData.Health));
        }

        public void SendCurrentServerState(NetworkConnectionToClient connection)
        {
            connection.Send(new MapNameResponse(MapProvider.MapName));
            SendMap(connection);
        }

        public void Stop()
        {
            _entityPositionValidator.Stop();
            _boxDropService.Stop();
            _spawnPointService.RemoveSpawnPoints();
            _fallDamageService.Stop();
            UnregisterHandlers();
        }

        private void RegisterHandlers()
        {
            _addBlocksHandler.Register();
            _changeClassHandler.Register();
            _changeSlotHandler.Register();
            _incrementSlotIndexHandler.Register();
            _decrementSlotIndexHandler.Register();
            _grenadeSpawnHandler.Register();
            _tntSpawnHandler.Register();
            _shootHandler.Register();
            _cancelShootHandler.Register();
            _reloadHandler.Register();
            _hitHandler.Register();
            _authenticationHandler.Register();
        }

        private void UnregisterHandlers()
        {
            _addBlocksHandler.Unregister();
            _changeClassHandler.Unregister();
            _changeSlotHandler.Unregister();
            _incrementSlotIndexHandler.Unregister();
            _decrementSlotIndexHandler.Unregister();
            _grenadeSpawnHandler.Unregister();
            _tntSpawnHandler.Unregister();
            _shootHandler.Unregister();
            _cancelShootHandler.Unregister();
            _reloadHandler.Unregister();
            _hitHandler.Unregister();
            _authenticationHandler.Unregister();
        }

        private void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            if (killer is not null && killer != victim)
            {
                GetPlayerData(killer).Kills += 1;
            }
        }

        public bool TryGetPlayerData(NetworkConnectionToClient connection, out PlayerData playerData)
        {
            if (connection is null)
            {
                playerData = null;
                return false;
            }

            return _dataByConnection.TryGetValue(connection, out playerData);
        }

        public PlayerData GetPlayerData(NetworkConnectionToClient connectionToClient)
        {
            return _dataByConnection[connectionToClient];
        }

        private void SendMap(NetworkConnectionToClient connection)
        {
            using var memoryStream = new MemoryStream();
            MapWriter.WriteMap(MapProvider, memoryStream);
            var bytes = memoryStream.ToArray();
            var mapMessages = MessageSplitter.SplitBytesIntoMessages(bytes, Constants.MessageSize);
            _networkManager.StartCoroutine(
                MessageSplitter.SendMessages(mapMessages, Constants.MessageDelay, false, connection));
        }

        private void SendDataFromOtherPlayers(NetworkConnectionToClient connection)
        {
            var playerData = GetPlayerData(connection);
            foreach (var anotherClient in ClientConnections)
            {
                if (anotherClient.identity == null)
                {
                    continue;
                }

                if (TryGetPlayerData(anotherClient, out var anotherPlayer) && anotherPlayer.IsAlive)
                {
                    connection.Send(new ChangeItemModelResponse(anotherClient.identity,
                        anotherPlayer.SelectedItem.id));
                    connection.Send(new NickNameResponse(anotherClient.identity, anotherPlayer.NickName));
                }
            }
        }

        private void RespawnPlayer(NetworkConnectionToClient connection)
        {
            var result = TryGetPlayerData(connection, out var playerData);
            if (!result)
            {
                return;
            }

            playerData.PlayerStateMachine.Enter<LifeState>();
            var player = _playerFactory.CreatePlayer(_spawnPointService.GetSpawnPosition());
            ReplacePlayer(connection, player);
            connection.Send(new PlayerConfigureResponse(playerData.Characteristic.placeDistance,
                playerData.Characteristic.speed, playerData.Characteristic.jumpHeight,
                playerData.Items.Select(item => item.id).ToList(),
                playerData.Health));
            NetworkServer.SendToReady(new NickNameResponse(connection.identity, playerData.NickName));
        }

        private void ReplacePlayer(NetworkConnectionToClient connection, GameObject newPlayer)
        {
            if (connection.identity == null)
            {
                return;
            }

            var oldPlayer = connection.identity.gameObject;
            NetworkServer.ReplacePlayerForConnection(connection, newPlayer, true);
            _networkManager.StartCoroutine(Utils.DoActionAfterDelay(() => DestroyPlayer(oldPlayer), 0.1f));
        }

        private void DestroyPlayer(GameObject oldPlayer)
        {
            if (oldPlayer != null)
            {
                NetworkServer.Destroy(oldPlayer);
            }
        }

        private List<ScoreData> GetScoreData()
        {
            var scoreData = new SortedSet<ScoreData>();
            foreach (var playerData in _dataByConnection.Values)
            {
                scoreData.Add(new ScoreData(playerData.SteamID, playerData.NickName, playerData.Kills,
                    playerData.Deaths, playerData.GameClass));
            }

            return scoreData.ToList();
        }
    }
}