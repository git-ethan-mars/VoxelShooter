using System.Collections.Generic;
using Data;
using Infrastructure.Services.StaticData;
using MapLogic;
using Mirror;
using Networking;
using Networking.ServerServices;
using PlayerLogic.States;
using Steamworks;

namespace Tests.EditMode
{
    public class MockServer : IServer
    {
        private readonly MapHistory _mapHistory;
        private readonly IStaticDataService _staticDataService;
        public IMapProvider MapProvider { get; set; }
        public IMapUpdater MapUpdater { get; }
        public BlockHealthSystem BlockHealthSystem { get; }
        public EntityContainer EntityContainer { get; } = new EntityContainer();

        public IEnumerable<NetworkConnectionToClient> ClientConnections { get; } =
            new List<NetworkConnectionToClient>();

        private readonly Dictionary<NetworkConnectionToClient, PlayerData> _playerDataByConnection;
        public string MapName { get; }

        public MockServer(IMapProvider mapProvider, MapHistory mapHistory,
            IStaticDataService staticDataService, MapMeshUpdater mapMeshUpdater)
        {
            _mapHistory = mapHistory;
            _staticDataService = staticDataService;
            _playerDataByConnection = new Dictionary<NetworkConnectionToClient, PlayerData>();
            MapProvider = mapProvider;
            MapUpdater = new MockMapUpdater(mapProvider, mapHistory, mapMeshUpdater);
            BlockHealthSystem = new BlockHealthSystem(_staticDataService, mapProvider);
        }

        public void Start()
        {
            _mapHistory.Start();
        }

        public void Stop()
        {
            _mapHistory.Stop();
        }

        public PlayerData GetPlayerData(NetworkConnectionToClient connectionToClient)
        {
            return _playerDataByConnection[connectionToClient];
        }

        public bool TryGetPlayerData(NetworkConnectionToClient connection, out PlayerData playerData)
        {
            return _playerDataByConnection.TryGetValue(connection, out playerData);
        }

        public void AddPlayer(NetworkConnectionToClient connection, CSteamID steamID, string nickname)
        {
            _playerDataByConnection[connection] = new PlayerData(steamID, nickname, _staticDataService);
        }

        public void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass)
        {
            var playerData = _playerDataByConnection[connection];
            playerData.GameClass = chosenClass;
            playerData.PlayerStateMachine.Enter<LifeState>();
        }

        public void DeletePlayer(NetworkConnectionToClient connection)
        {
        }

        public void SendCurrentServerState(NetworkConnectionToClient connection)
        {
        }

        public void Heal(NetworkConnectionToClient receiver, int totalHeal)
        {
        }

        public void Damage(NetworkConnectionToClient source, NetworkConnectionToClient receiver, int totalDamage)
        {
        }
    }
}