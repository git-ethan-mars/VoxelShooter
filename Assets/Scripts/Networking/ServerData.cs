using System.Collections.Generic;
using System.Linq;
using Data;
using Entities;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.ObjectCorrectors;
using Infrastructure.Services.StaticData;
using MapLogic;
using Mirror;
using Networking.Messages;
using PlayerLogic.States;
using Steamworks;
using UnityEngine;

namespace Networking
{
    public class ServerData
    {
        public MapProvider MapProvider { get; }
        public readonly MapUpdater MapUpdater;
        public readonly List<KillData> KillStatistics;
        private readonly Dictionary<NetworkConnectionToClient, PlayerData> _dataByConnection;
        private readonly IStaticDataService _staticData;
        private readonly IPlayerFactory _playerFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ServerSettings _serverSettings;
        private readonly ObjectPositionValidator _objectPositionValidator;
        private readonly IEntityFactory _entityFactory;

        public ServerData(ICoroutineRunner coroutineRunner, IAssetProvider assets, IStaticDataService staticDataService,
            IParticleFactory particleFactory, MapProvider mapProvider, ServerSettings serverSettings,
            IEntityFactory entityFactory)
        {
            _coroutineRunner = coroutineRunner;
            _dataByConnection = new Dictionary<NetworkConnectionToClient, PlayerData>();
            KillStatistics = new List<KillData>();
            _staticData = staticDataService;
            MapProvider = mapProvider;
            MapUpdater = new MapUpdater(mapProvider);
            _serverSettings = serverSettings;
            _entityFactory = entityFactory;
            _playerFactory = new PlayerFactory(assets, this, particleFactory);
            _objectPositionValidator = new ObjectPositionValidator(MapUpdater, MapProvider);
            AddSpawnPoint();
        }

        private void AddSpawnPoint()
        {
            foreach (var spawnPosition in MapProvider.MapData.SpawnPoints)
            {
                Debug.Log(spawnPosition);
                var spawnPoint = _entityFactory.CreateSpawnPoint(spawnPosition.ToUnityVector());
                _objectPositionValidator.AddPushable(spawnPoint.GetComponent<PushableObject>());
            }
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, CSteamID steamID,
            string nickname)
        {
            var playerData = new PlayerData(steamID, nickname, _staticData);
            playerData.GameClass = chosenClass;
            _dataByConnection[connection] = playerData;
            _playerFactory.CreatePlayer(connection);
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
        }

        public void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
        {
            var playerData = _dataByConnection[connection];
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

            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            _dataByConnection.Remove(connection);
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
        }


        public void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            var tombstonePosition = Vector3Int.FloorToInt(victim.identity.transform.position) +
                                    new Vector3(0.5f, 0.5f, 0.5f);
            Debug.Log(tombstonePosition);
            var tombstone = _entityFactory.CreateTombstone(tombstonePosition);
            _objectPositionValidator.AddPushable(tombstone.GetComponent<PushableObject>());
            if (killer is not null && killer != victim)
                _dataByConnection[killer].Kills += 1;
            KillStatistics.Add(new KillData(killer, victim));
            var playerData = _dataByConnection[victim];
            playerData.PlayerStateMachine.Enter<DeathState>();
            _playerFactory.CreateSpectatorPlayer(victim);
            var respawnTimer = new RespawnTimer(_coroutineRunner, victim, _serverSettings.SpawnTime,
                () => _playerFactory.RespawnPlayer(victim));
            respawnTimer.Start();
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
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

        public List<KeyValuePair<NetworkConnectionToClient, PlayerData>> GetAlivePlayers(
            NetworkConnectionToClient except)
        {
            return _dataByConnection.Where(kvp => kvp.Value.IsAlive && except != kvp.Key).ToList();
        }

        public IEnumerable<NetworkConnectionToClient> GetConnections()
        {
            return _dataByConnection.Keys.ToArray();
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