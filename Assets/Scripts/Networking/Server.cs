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
using Vector3 = UnityEngine.Vector3;

namespace Networking
{
    public class Server : IServer
    {
        public IMapProvider MapProvider { get; set; }
        public ServerData ServerData { get; set; }
        public IMapUpdater MapUpdater { get; set; }
        private readonly ServerSettings _serverSettings;
        private readonly ObjectPositionValidator _objectPositionValidator;
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
            ServerData = new ServerData(staticDataService);
            _objectPositionValidator = new ObjectPositionValidator(MapUpdater, MapProvider);
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
                                    new Vector3(0.5f, 0.5f, 0.5f);
            var tombstone = _entityFactory.CreateTombstone(tombstonePosition);
            _objectPositionValidator.AddPushable(tombstone.GetComponent<PushableObject>());
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
            foreach (var spawnPosition in MapProvider.MapData.SpawnPoints)
            {
                var spawnPoint = _entityFactory.CreateSpawnPoint(spawnPosition.ToUnityVector());
                _objectPositionValidator.AddPushable(spawnPoint.GetComponent<PushableObject>());
            }
        }
    }
}