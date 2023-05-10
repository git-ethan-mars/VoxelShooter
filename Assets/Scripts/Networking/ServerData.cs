using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using MapLogic;
using Mirror;

namespace Networking
{
    public class ServerData
    {
        public readonly Dictionary<NetworkConnectionToClient, PlayerData> DataByConnection;
        public Map Map { get; }
        private readonly IStaticDataService _staticData;
        public readonly List<KillData> Kills;


        public ServerData(IStaticDataService staticDataService, Map map)
        {
            DataByConnection = new Dictionary<NetworkConnectionToClient, PlayerData>();
            Kills = new List<KillData>();
            _staticData = staticDataService;
            Map = map;
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, string nick)
        {
            DataByConnection[connection] = new PlayerData(chosenClass, nick, _staticData);
        }
        

        public PlayerData GetPlayerData(NetworkConnectionToClient connection)
        {
            return DataByConnection.TryGetValue(connection, out var playerData) ? playerData : null;
        }

        public void UpdatePlayerClass(NetworkConnectionToClient conn, GameClass newClass)
        {
            DataByConnection[conn] = new PlayerData(newClass, DataByConnection[conn].NickName, _staticData);
        }

        public int GetItemCount(NetworkConnectionToClient connection, int itemId)
        {
            var playerData = GetPlayerData(connection);
            return playerData.ItemCountById[itemId];
        }

        public void SetItemCount(NetworkConnectionToClient connection, int itemId, int value)
        {
            var playerData = GetPlayerData(connection);
            playerData.ItemCountById[itemId] = value;
        }

        public void AddKill(NetworkConnectionToClient killer, NetworkConnectionToClient victim)
        {
            if (killer is not null)
                DataByConnection[killer].PlayerStatistic.Kills += 1;
            DataByConnection[victim].PlayerStatistic.Deaths += 1;
            Kills.Add(new KillData(killer,victim));
        }
    }
}