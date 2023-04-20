using System.Collections.Generic;
using Data;
using Infrastructure.Services;
using Mirror;

namespace Networking
{
    public class ServerData
    {
        private readonly IStaticDataService _staticData;
        private readonly Dictionary<int, PlayerData> _dataByConnectionId;


        public ServerData(IStaticDataService staticDataService)
        {
            _dataByConnectionId = new Dictionary<int, PlayerData>();
            _staticData = staticDataService;
        }

        public void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, string nick)
        {
            _dataByConnectionId[connection.connectionId] = new PlayerData(chosenClass, nick, _staticData);
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
            _dataByConnectionId.Remove(connection.connectionId);
        }

        public PlayerData GetPlayerData(int id)
        {
            return _dataByConnectionId.TryGetValue(id, out var playerData) ? playerData : null;
        }

        public void UpdatePlayer(NetworkConnectionToClient conn)
        {
            var chosenClass = _dataByConnectionId[conn.connectionId].GameClass;
            var nickName = _dataByConnectionId[conn.connectionId].NickName;
            _dataByConnectionId[conn.connectionId] = new PlayerData(chosenClass, nickName, _staticData);
        }
    }
}