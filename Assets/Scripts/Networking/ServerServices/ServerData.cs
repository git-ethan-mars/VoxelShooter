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
        private readonly Dictionary<NetworkConnectionToClient, PlayerData> _dataByConnection;
        public readonly IStaticDataService StaticData;

        public ServerData(IStaticDataService staticDataService)
        {
            _dataByConnection = new Dictionary<NetworkConnectionToClient, PlayerData>();
            StaticData = staticDataService;
        }

        public IEnumerable<NetworkConnectionToClient> ClientConnections => _dataByConnection.Keys.ToArray();

        public void AddPlayer(NetworkConnectionToClient connection, CSteamID steamID,
            string nickname)
        {
            var playerData = new PlayerData(steamID, nickname, StaticData);
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