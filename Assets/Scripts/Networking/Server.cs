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
        public IMapProvider MapProvider { get; }
        public ServerData ServerData { get; }
        public IMapUpdater MapUpdater { get; }
        public MapDestructionAlgorithm MapDestructionAlgorithm { get; }
        private readonly ServerSettings _serverSettings;
        private readonly EntityPositionValidator _entityPositionValidator;
        private readonly IEntityFactory _entityFactory;
        private readonly IPlayerFactory _playerFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AddBlocksHandler _addBlocksHandler;
        private readonly ChangeClassHandler _changeClassHandler;
        private readonly GrenadeSpawnHandler _grenadeSpawnHandler;
        private readonly TntSpawnHandler _tntSpawnHandler;
        private readonly ChangeSlotHandler _changeSlotHandler;

        public Server(ICoroutineRunner coroutineRunner, IStaticDataService staticData,
            ServerSettings serverSettings, IAssetProvider assets,
            IParticleFactory particleFactory, IEntityFactory entityFactory)
        {
            _coroutineRunner = coroutineRunner;
            _serverSettings = serverSettings;
            _entityFactory = entityFactory;
            MapProvider = MapReader.ReadFromFile(_serverSettings.MapName);
            MapUpdater = new MapUpdater(MapProvider);
            MapDestructionAlgorithm = new MapDestructionAlgorithm(MapProvider, MapUpdater);
            ServerData = new ServerData(staticData, MapProvider);
            _entityPositionValidator = new EntityPositionValidator(MapUpdater, MapProvider);
            _playerFactory = new PlayerFactory(assets, this, particleFactory);
            var sphereExplosionArea = new SphereExplosionArea(MapProvider);
            var singleExplosionBehaviour = new SingleExplosionBehaviour(MapUpdater, particleFactory,
                sphereExplosionArea, MapDestructionAlgorithm);
            var chainExplosionBehaviour = new ChainExplosionBehaviour(MapUpdater, particleFactory,
                sphereExplosionArea, MapDestructionAlgorithm);
            _addBlocksHandler = new AddBlocksHandler(this);
            _changeClassHandler = new ChangeClassHandler(this);
            _changeSlotHandler = new ChangeSlotHandler(this);
            _grenadeSpawnHandler = new GrenadeSpawnHandler(this, coroutineRunner, entityFactory, staticData,
                singleExplosionBehaviour);
            _tntSpawnHandler =
                new TntSpawnHandler(this, coroutineRunner, entityFactory, staticData, chainExplosionBehaviour);
        }

        public void RegisterHandlers()
        {
            _addBlocksHandler.Register();
            _changeClassHandler.Register();
            _changeSlotHandler.Register();
            _grenadeSpawnHandler.Register();
            _tntSpawnHandler.Register();
        }

        public void UnregisterHandlers()
        {
            _addBlocksHandler.Unregister();
            _changeClassHandler.Unregister();
            _changeSlotHandler.Unregister();
            _grenadeSpawnHandler.Unregister();
            _tntSpawnHandler.Unregister();
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, CSteamID steamID,
            string nickname)
        {
            ServerData.AddPlayer(connection, chosenClass, steamID, nickname);
            _playerFactory.CreatePlayer(connection);
            NetworkServer.SendToAll(new ScoreboardResponse(ServerData.GetScoreData()));
        }

        public void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
        {
            var playerData = ServerData.GetPlayerData(connection);
            if (playerData.GameClass == chosenClass) return;
            playerData.GameClass = chosenClass;
            if (playerData.IsAlive)
            {
                playerData.PlayerStateMachine.Enter<DeathState>();
                _playerFactory.CreateSpectatorPlayer(connection);
                var respawnTimer = new RespawnTimer(_coroutineRunner, connection, _serverSettings.SpawnTime,
                    () => _playerFactory.RespawnPlayer(connection));
                respawnTimer.Start();
            }

            NetworkServer.SendToAll(new ScoreboardResponse(ServerData.GetScoreData()));
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            ServerData.DeletePlayer(connection);
            NetworkServer.SendToAll(new ScoreboardResponse(ServerData.GetScoreData()));
        }

        public void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            var tombstonePosition = Vector3Int.FloorToInt(victim.identity.transform.position) +
                                    Constants.WorldOffset;
            var tombstone = _entityFactory.CreateTombstone(tombstonePosition);
            _entityPositionValidator.AddEntity(tombstone.GetComponent<PushableObject>());
            ServerData.AddKill(killer, victim);
            var playerData = ServerData.GetPlayerData(victim);
            playerData.PlayerStateMachine.Enter<DeathState>();
            _playerFactory.CreateSpectatorPlayer(victim);
            var respawnTimer = new RespawnTimer(_coroutineRunner, victim, _serverSettings.SpawnTime,
                () => _playerFactory.RespawnPlayer(victim));
            respawnTimer.Start();
            NetworkServer.SendToAll(new ScoreboardResponse(ServerData.GetScoreData()));
        }

        public void CreateSpawnPoints(Transform parent)
        {
            foreach (var spawnPointData in MapProvider.MapData.SpawnPoints)
            {
                var spawnPointScript = _entityFactory.CreateSpawnPoint(spawnPointData.ToVectorWithOffset(), parent)
                    .GetComponent<SpawnPoint>();
                spawnPointScript.Construct(spawnPointData);
                _entityPositionValidator.AddEntity(spawnPointScript);
                spawnPointScript.PositionUpdated += MapUpdater.UpdateSpawnPoint;
            }
        }
    }
}