using System.IO;
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
using ChangeSlotHandler = Networking.MessageHandlers.RequestHandlers.ChangeSlotHandler;
using ShootHandler = Networking.MessageHandlers.RequestHandlers.ShootHandler;

namespace Networking
{
    public class Server : IServer
    {
        private const string SpawnPointContainerName = "SpawnPointContainer";
        public MapProvider MapProvider { get; }
        public ServerData Data { get; }
        public MapUpdater MapUpdater { get; }
        private readonly IGameFactory _gameFactory;
        private readonly ServerSettings _serverSettings;
        private readonly EntityPositionValidator _entityPositionValidator;
        private readonly IEntityFactory _entityFactory;
        private readonly IPlayerFactory _playerFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AddBlocksHandler _addBlocksHandler;
        private readonly ChangeClassHandler _changeClassHandler;
        private readonly GrenadeSpawnHandler _grenadeSpawnHandler;
        private readonly RocketSpawnHandler _rocketSpawnHandler;
        private readonly TntSpawnHandler _tntSpawnHandler;
        private readonly ChangeSlotHandler _changeSlotHandler;
        private readonly ShootHandler _shootHandler;
        private readonly ReloadHandler _reloadHandler;
        private readonly HitHandler _hitHandler;
        private readonly AuthenticationHandler _authenticationHandler;

        public Server(ICoroutineRunner coroutineRunner, IStaticDataService staticData,
            ServerSettings serverSettings, IAssetProvider assets, IGameFactory gameFactory,
            IParticleFactory particleFactory, IEntityFactory entityFactory)
        {
            _coroutineRunner = coroutineRunner;
            _serverSettings = serverSettings;
            _gameFactory = gameFactory;
            _entityFactory = entityFactory;
            MapProvider = MapReader.ReadFromFile(_serverSettings.MapName, staticData);
            MapUpdater = new MapUpdater(_coroutineRunner, MapProvider);
            Data = new ServerData(staticData);
            _entityPositionValidator = new EntityPositionValidator(MapUpdater, MapProvider);
            _playerFactory = new PlayerFactory(this, assets);
            var sphereExplosionArea = new SphereExplosionArea(MapProvider);
            var singleExplosionBehaviour = new SingleExplosionBehaviour(this, particleFactory,
                sphereExplosionArea);
            var chainExplosionBehaviour = new ChainExplosionBehaviour(this, particleFactory,
                sphereExplosionArea);
            _addBlocksHandler = new AddBlocksHandler(this);
            _changeClassHandler = new ChangeClassHandler(this);
            _changeSlotHandler = new ChangeSlotHandler(this);
            _grenadeSpawnHandler = new GrenadeSpawnHandler(this, coroutineRunner, entityFactory, staticData,
                singleExplosionBehaviour);
            _rocketSpawnHandler = new RocketSpawnHandler(this, staticData, entityFactory, particleFactory);
            _tntSpawnHandler =
                new TntSpawnHandler(this, coroutineRunner, entityFactory, staticData, chainExplosionBehaviour);
            var rangeWeaponValidator = new RangeWeaponValidator(this, coroutineRunner, particleFactory);
            var meleeWeaponValidator = new MeleeWeaponValidator(this, coroutineRunner, particleFactory);
            _shootHandler = new ShootHandler(this, rangeWeaponValidator);
            _reloadHandler = new ReloadHandler(this, rangeWeaponValidator);
            var boxDropService = new BoxDropService(this, _coroutineRunner, _serverSettings.MaxDuration, _entityFactory,
                _entityPositionValidator, _gameFactory);
            boxDropService.Start();
            _hitHandler = new HitHandler(this, meleeWeaponValidator);
            _authenticationHandler = new AuthenticationHandler(this);
        }

        public void Start()
        {
            RegisterHandlers();
            CreateSpawnPoints();
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
                    playerData.Characteristic.speed, playerData.Characteristic.jumpHeight, playerData.ItemIds,
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
                var spectator = _playerFactory.CreateSpectatorPlayer();
                ReplacePlayer(connection, spectator);
                var respawnTimer = new RespawnTimer(_coroutineRunner, connection, _serverSettings.SpawnTime,
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
            UnregisterHandlers();
        }

        private void RegisterHandlers()
        {
            _addBlocksHandler.Register();
            _changeClassHandler.Register();
            _changeSlotHandler.Register();
            _grenadeSpawnHandler.Register();
            _rocketSpawnHandler.Register();
            _tntSpawnHandler.Register();
            _shootHandler.Register();
            _reloadHandler.Register();
            _hitHandler.Register();
            _authenticationHandler.Register();
        }

        private void UnregisterHandlers()
        {
            _addBlocksHandler.Unregister();
            _changeClassHandler.Unregister();
            _changeSlotHandler.Unregister();
            _grenadeSpawnHandler.Unregister();
            _rocketSpawnHandler.Unregister();
            _tntSpawnHandler.Unregister();
            _shootHandler.Unregister();
            _reloadHandler.Unregister();
            _hitHandler.Unregister();
            _authenticationHandler.Unregister();
        }

        private void CreateSpawnPoints()
        {
            var parent = _gameFactory.CreateGameObjectContainer(SpawnPointContainerName).transform;
            foreach (var spawnPointData in MapProvider.SceneData.SpawnPoints)
            {
                var spawnPointScript = _entityFactory.CreateSpawnPoint(spawnPointData.ToVectorWithOffset(), parent)
                    .GetComponent<SpawnPoint>();
                spawnPointScript.Construct(spawnPointData);
                _entityPositionValidator.AddEntity(spawnPointScript);
                spawnPointScript.PositionUpdated += MapUpdater.UpdateSpawnPoint; // TODO : Need to unsubscribe
            }
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
            var spectatorPlayer = _playerFactory.CreateSpectatorPlayer();
            ReplacePlayer(victim, spectatorPlayer);
            var respawnTimer = new RespawnTimer(_coroutineRunner, victim, _serverSettings.SpawnTime,
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
                        anotherPlayer.ItemIds[anotherPlayer.InventorySlotId]));
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
                playerData.Characteristic.speed, playerData.Characteristic.jumpHeight, playerData.ItemIds, playerData.Health));
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