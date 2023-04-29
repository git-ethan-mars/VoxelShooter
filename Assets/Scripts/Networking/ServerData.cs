using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using MapLogic;
using Mirror;

namespace Networking
{
    public class ServerData
    {
        public Map Map { get; set; }
        private readonly IStaticDataService _staticData;
        private readonly Dictionary<int, PlayerData> _dataByConnectionId;
        


        public ServerData(IStaticDataService staticDataService, Map map)
        {
            _dataByConnectionId = new Dictionary<int, PlayerData>();
            _staticData = staticDataService;
            Map = map;
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, string nick)
        {
            _dataByConnectionId[connection.connectionId] = new PlayerData(chosenClass, nick, _staticData);
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            _dataByConnectionId.Remove(connection.connectionId);
        }

        public PlayerData GetPlayerData(NetworkConnectionToClient connection)
        {
            return _dataByConnectionId.TryGetValue(connection.connectionId, out var playerData) ? playerData : null;
        }

        public void UpdatePlayer(NetworkConnectionToClient conn)
        {
            var chosenClass = _dataByConnectionId[conn.connectionId].GameClass;
            var nickName = _dataByConnectionId[conn.connectionId].NickName;
            _dataByConnectionId[conn.connectionId] = new PlayerData(chosenClass, nickName, _staticData);
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
    }
}