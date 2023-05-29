using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure;
using Infrastructure.AssetManagement;
using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using MapLogic;
using Mirror;
using Networking.Messages;
using PlayerLogic.States;
using Steamworks;

namespace Networking
{
    public class ServerData
    {
        public readonly Dictionary<NetworkConnectionToClient, PlayerData> DataByConnection;
        public Map Map { get; }
        public readonly List<KillData> KillStatistics;
        private readonly IStaticDataService _staticData;
        private readonly IPlayerFactory _playerFactory;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ServerSettings _serverSettings;

        public ServerData(ICoroutineRunner coroutineRunner, IAssetProvider assets, IStaticDataService staticDataService,
            IParticleFactory particleFactory, Map map, ServerSettings serverSettings, IEntityFactory entityFactory)
        {
            _coroutineRunner = coroutineRunner;
            DataByConnection = new Dictionary<NetworkConnectionToClient, PlayerData>();
            KillStatistics = new List<KillData>();
            _staticData = staticDataService;
            Map = map;
            _serverSettings = serverSettings;
            _playerFactory = new PlayerFactory(assets, this, particleFactory, entityFactory);
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, CSteamID steamID,
            string nickname)
        {
            var playerData = new PlayerData(steamID, nickname, _staticData);
            playerData.GameClass = chosenClass;
            playerData.PlayerStateMachine.Enter<LifeState>();
            DataByConnection[connection] = playerData;
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
            _playerFactory.CreatePlayer(connection);
        }

        public void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
        {
            var playerData = DataByConnection[connection];
            if (playerData.GameClass == chosenClass) return;
            playerData.GameClass = chosenClass;
            if (playerData.IsAlive)
            {
                playerData.PlayerStateMachine.Enter<DeathState>();
                _playerFactory.CreateSpectatorPlayer(connection);
                var respawnTimer = new RespawnTimer(_coroutineRunner, connection, _serverSettings.SpawnTime,
                    () => Respawn(connection, playerData));
                respawnTimer.Start();
            }
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));

        }

        private void Respawn(NetworkConnectionToClient connection, PlayerData playerData)
        {
            playerData.PlayerStateMachine.Enter<LifeState>();
            _playerFactory.RespawnPlayer(connection, playerData);
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            DataByConnection.Remove(connection);
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
        }


        public void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            if (killer is not null && killer != victim)
                DataByConnection[killer].Kills += 1;
            KillStatistics.Add(new KillData(killer, victim));
            var playerData = DataByConnection[victim];
            playerData.PlayerStateMachine.Enter<DeathState>();
            _playerFactory.CreateSpectatorPlayer(victim);
            var respawnTimer = new RespawnTimer(_coroutineRunner, victim, _serverSettings.SpawnTime,
                () => Respawn(victim, playerData));
            respawnTimer.Start();
            NetworkServer.SendToAll(new ScoreboardMessage(GetScoreData()));
        }

        public PlayerData GetPlayerData(NetworkConnectionToClient connection)
        {
            return DataByConnection.TryGetValue(connection, out var playerData) ? playerData : null;
        }

        public int GetItemCount(NetworkConnectionToClient connection, int itemId)
        {
            var playerData = DataByConnection[connection];
            return playerData.ItemCountById[itemId];
        }

        public void SetItemCount(NetworkConnectionToClient connection, int itemId, int value)
        {
            var playerData = DataByConnection[connection];
            playerData.ItemCountById[itemId] = value;
        }

        private List<ScoreData> GetScoreData()
        {
            var scoreData = new SortedSet<ScoreData>();
            foreach (var playerData in DataByConnection.Values)
            {
                scoreData.Add(new ScoreData(playerData.SteamID, playerData.NickName, playerData.Kills,
                    playerData.Deaths, playerData.GameClass));
            }

            return scoreData.ToList();
        }
    }
}