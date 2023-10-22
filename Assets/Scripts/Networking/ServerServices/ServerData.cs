using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.Services.StaticData;
using Mirror;
using Steamworks;

namespace Networking.ServerServices
{
    public class ServerData
    {
        public readonly List<KillData> KillStatistics;
        private readonly Dictionary<NetworkConnectionToClient, PlayerData> _dataByConnection;
        private readonly IStaticDataService _staticData;

        public ServerData(IStaticDataService staticDataService)
        {
            _dataByConnection = new Dictionary<NetworkConnectionToClient, PlayerData>();
            KillStatistics = new List<KillData>();
            _staticData = staticDataService;
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, CSteamID steamID,
            string nickname)
        {
            var playerData = new PlayerData(steamID, nickname, _staticData);
            playerData.GameClass = chosenClass;
            _dataByConnection[connection] = playerData;
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            _dataByConnection.Remove(connection);
        }

        public void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            if (killer is not null && killer != victim)
            {
                GetPlayerData(killer).Kills += 1;
            }

            KillStatistics.Add(new KillData(killer, victim));
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

        public List<ScoreData> GetScoreData()
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