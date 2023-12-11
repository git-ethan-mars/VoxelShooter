using System.IO;
using System.Linq;
using Data;
using Entities;
using Explosions;
using Infrastructure;
using Infrastructure.AssetManagement;
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
        public ServerData Data { get; }
        public BlockHealthSystem BlockHealthSystem { get; }

        private readonly ServerSettings _serverSettings;
        private readonly EntityPositionValidator _entityPositionValidator;
        private readonly BoxDropService _boxDropService;
        private readonly SpawnPointService _spawnPointService;
        private readonly ServerTimer _serverTimer;
        private readonly IEntityFactory _entityFactory;
        private readonly IPlayerFactory _playerFactory;
        private readonly ICoroutineRunner _customNetworkManager;
        private readonly AddBlocksHandler _addBlocksHandler;
        private readonly ChangeClassHandler _changeClassHandler;
        private readonly GrenadeSpawnHandler _grenadeSpawnHandler;
        private readonly RocketSpawnHandler _rocketSpawnHandler;
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

        public Server(CustomNetworkManager customNetworkManager, IStaticDataService staticData,
            ServerSettings serverSettings, IAssetProvider assets, IGameFactory gameFactory,
            IParticleFactory particleFactory, IEntityFactory entityFactory)
        {
            _customNetworkManager = customNetworkManager;
            _serverSettings = serverSettings;
            _entityFactory = entityFactory;
            MapProvider = MapReader.ReadFromFile(serverSettings.MapName, staticData);
            var mapUpdater = new MapUpdater(customNetworkManager, MapProvider);
            _entityPositionValidator = new EntityPositionValidator(mapUpdater, MapProvider);
            _spawnPointService =
                new SpawnPointService(MapProvider, gameFactory, entityFactory, _entityPositionValidator);
            _playerFactory = new PlayerFactory(assets, _spawnPointService);
            Data = new ServerData(staticData);
            _serverTimer = new ServerTimer(customNetworkManager, serverSettings.MaxDuration);
            _boxDropService = new BoxDropService(this, customNetworkManager, serverSettings, entityFactory,
                _entityPositionValidator, gameFactory);
            var sphereExplosionArea = new SphereDamageArea(MapProvider);
            var singleExplosionBehaviour = new SingleExplosionBehaviour(this, particleFactory,
                sphereExplosionArea);
            var chainExplosionBehaviour = new ChainExplosionBehaviour(this, particleFactory,
                sphereExplosionArea);
            BlockHealthSystem = new BlockHealthSystem(staticData, MapProvider, mapUpdater);
            var audioService = new AudioService(staticData);
            var rangeWeaponValidator = new RangeWeaponValidator(this, customNetworkManager, particleFactory, audioService);
            var meleeWeaponValidator = new MeleeWeaponValidator(this, customNetworkManager, particleFactory, audioService);
            _addBlocksHandler = new AddBlocksHandler(this);
            _changeClassHandler = new ChangeClassHandler(this);
            _changeSlotHandler = new ChangeSlotHandler(this, audioService);
            _incrementSlotIndexHandler = new IncrementSlotIndexHandler(this, audioService);
            _decrementSlotIndexHandler = new DecrementSlotIndexHandler(this, audioService);
            _grenadeSpawnHandler = new GrenadeSpawnHandler(this, customNetworkManager, entityFactory,
                singleExplosionBehaviour, audioService);
            _rocketSpawnHandler = new RocketSpawnHandler(this, staticData, entityFactory, particleFactory, audioService);
            _tntSpawnHandler =
                new TntSpawnHandler(this, customNetworkManager, entityFactory, chainExplosionBehaviour);
            _shootHandler = new ShootHandler(this, rangeWeaponValidator);
            _cancelShootHandler = new CancelShootHandler(this, rangeWeaponValidator);
            _reloadHandler = new ReloadHandler(this, rangeWeaponValidator);
            _hitHandler = new HitHandler(this, meleeWeaponValidator);
            _authenticationHandler = new AuthenticationHandler(this);
            _fallDamageService = new FallDamageService(this, customNetworkManager);
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
            Data.AddPlayer(connection, steamID, nickname);
        }

        public void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
        {
            var playerData = Data.GetPlayerData(connection);
            if (playerData.GameClass == GameClass.None)
            {
                playerData.GameClass = chosenClass;
                playerData.PlayerStateMachine.Enter<LifeState>();
                var player = _playerFactory.CreatePlayer();
                NetworkServer.AddPlayerForConnection(connection, player);
                connection.Send(new PlayerConfigureResponse(playerData.Characteristic.placeDistance,
                    playerData.Characteristic.speed, playerData.Characteristic.jumpHeight,
                    playerData.Items.Select(item => item.id).ToList(),
                    playerData.Health));
                SendDataFromOtherPlayers(connection);
                NetworkServer.SendToReady(new NickNameResponse(connection.identity, playerData.NickName));
            }
            else
            {
                playerData.GameClass = chosenClass;
                if (!playerData.IsAlive)
                {
                    return;
                }

                playerData.PlayerStateMachine.Enter<DeathState>();
                var spectator = _playerFactory.CreateSpectatorPlayer(connection.identity.transform.position);
                ReplacePlayer(connection, spectator);
                connection.Send(new SpectatorConfigureResponse());
                var respawnTimer = new RespawnTimer(_customNetworkManager, connection, _serverSettings.SpawnTime,
                    () => RespawnPlayer(connection));
                respawnTimer.Start();
            }

            NetworkServer.SendToReady(new ScoreboardResponse(Data.GetScoreData()));
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            Data.DeletePlayer(connection);
            NetworkServer.SendToReady(new ScoreboardResponse(Data.GetScoreData()));
            NetworkServer.DestroyPlayerForConnection(connection);
        }

        public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
        {
            var result = Data.TryGetPlayerData(receiver, out var playerData);
            Debug.Log(11111111);
            if (!result || !playerData.IsAlive) return;
            playerData.Health -= totalDamage;
            if (playerData.Health <= 0)
            {
                playerData.Health = 0;
                AddKill(source, receiver);
            }
            else
            {
                receiver.Send(new HealthResponse(playerData.Health));
            }
        }

        public void Heal(NetworkConnectionToClient receiver, int totalHeal)
        {
            var result = Data.TryGetPlayerData(receiver, out var playerData);
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
            _rocketSpawnHandler.Register();
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
            _rocketSpawnHandler.Unregister();
            _tntSpawnHandler.Unregister();
            _shootHandler.Unregister();
            _cancelShootHandler.Unregister();
            _reloadHandler.Unregister();
            _hitHandler.Unregister();
            _authenticationHandler.Unregister();
        }

        private void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            var tombstonePosition = Vector3Int.FloorToInt(victim.identity.transform.position) +
                                    Constants.worldOffset;
            var tombstone = _entityFactory.CreateTombstone(tombstonePosition);
            _entityPositionValidator.AddEntity(tombstone.GetComponent<IPushable>());
            Data.AddKill(killer, victim);
            var playerData = Data.GetPlayerData(victim);
            playerData.PlayerStateMachine.Enter<DeathState>();
            var spectatorPlayer = _playerFactory.CreateSpectatorPlayer(tombstonePosition);
            ReplacePlayer(victim, spectatorPlayer);
            victim.Send(new SpectatorConfigureResponse());
            var respawnTimer = new RespawnTimer(_customNetworkManager, victim, _serverSettings.SpawnTime,
                () => RespawnPlayer(victim));
            respawnTimer.Start();
            NetworkServer.SendToReady(new ScoreboardResponse(Data.GetScoreData()));
        }

        private void SendMap(NetworkConnectionToClient connection)
        {
            using var memoryStream = new MemoryStream();
            MapWriter.WriteMap(MapProvider, memoryStream);
            var bytes = memoryStream.ToArray();
            var mapSplitter = new MapSplitter();
            var mapMessages = mapSplitter.SplitBytesIntoMessages(bytes, Constants.MessageSize);
            mapSplitter.SendMessages(mapMessages, connection);
        }

        private void SendDataFromOtherPlayers(NetworkConnectionToClient connection)
        {
            var playerData = Data.GetPlayerData(connection);
            foreach (var anotherClient in Data.ClientConnections)
            {
                if (anotherClient.identity == null)
                {
                    continue;
                }

                if (Data.TryGetPlayerData(anotherClient, out var anotherPlayer) && playerData.IsAlive)
                {
                    connection.Send(new ChangeItemModelResponse(anotherClient.identity,
                        anotherPlayer.SelectedItem.id));
                    connection.Send(new NickNameResponse(anotherClient.identity, anotherPlayer.NickName));
                }
            }
        }

        private void RespawnPlayer(NetworkConnectionToClient connection)
        {
            var playerData = Data.GetPlayerData(connection);
            playerData.PlayerStateMachine.Enter<LifeState>();
            var player = _playerFactory.CreatePlayer();
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
            Object.Destroy(oldPlayer, 0.1f);
        }
    }
}