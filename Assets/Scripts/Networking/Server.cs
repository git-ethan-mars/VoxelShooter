using Data;
using Entities;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using MapLogic;
using Mirror;
using Networking.Messages;
using Networking.ServerServices;
using PlayerLogic.States;
using Steamworks;
using UnityEngine;

namespace Networking
{
    public class Server : IServer
    {
        public IMapProvider MapProvider { get; set; }
        public ServerData ServerData { get; set; }
        public IMapUpdater MapUpdater { get; set; }
        public Algorithm Algorithm { get; set; }
        private readonly ServerSettings _serverSettings;
        private readonly EntityPositionValidator _entityPositionValidator;
        private readonly IEntityFactory _entityFactory;
        private readonly IPlayerFactory _playerFactory;
        private readonly ICoroutineRunner _coroutineRunner;

        public Server(ICoroutineRunner coroutineRunner, IStaticDataService staticDataService,
            ServerSettings serverSettings, IAssetProvider assets,
            IParticleFactory particleFactory, IEntityFactory entityFactory)
        {
            _coroutineRunner = coroutineRunner;
            _serverSettings = serverSettings;
            _entityFactory = entityFactory;
            MapProvider = MapReader.ReadFromFile(_serverSettings.MapName + ".rch");
            MapUpdater = new MapUpdater(MapProvider);
            Algorithm = new Algorithm(MapProvider, MapUpdater);
            ServerData = new ServerData(staticDataService, MapProvider);
            _entityPositionValidator = new EntityPositionValidator(MapUpdater, MapProvider);
            _playerFactory = new PlayerFactory(assets, this, particleFactory);
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, CSteamID steamID,
            string nickname)
        {
            ServerData.AddPlayer(connection, chosenClass, steamID, nickname);
            _playerFactory.CreatePlayer(connection);
            NetworkServer.SendToAll(new ScoreboardMessage(ServerData.GetScoreData()));
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

            NetworkServer.SendToAll(new ScoreboardMessage(ServerData.GetScoreData()));
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            ServerData.DeletePlayer(connection);
            NetworkServer.SendToAll(new ScoreboardMessage(ServerData.GetScoreData()));
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
            NetworkServer.SendToAll(new ScoreboardMessage(ServerData.GetScoreData()));
        }

        public void CreateSpawnPoints()
        {
            foreach (var spawnPointData in MapProvider.MapData.SpawnPoints)
            {
                var spawnPoint = _entityFactory.CreateSpawnPoint(spawnPointData.ToVectorWithOffset()).GetComponent<SpawnPoint>();
                spawnPoint.Construct(spawnPointData);
                _entityPositionValidator.AddEntity(spawnPoint);
                spawnPoint.PositionUpdated += MapUpdater.UpdateSpawnPoint;
            }
        }
    }
}