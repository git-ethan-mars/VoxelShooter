﻿using Data;
using MapLogic;
using Mirror;
using Networking.ServerServices;
using Steamworks;

namespace Networking
{
    public interface IServer
    {
        IMapProvider MapProvider { get; }
        ServerData ServerData { get; }
        IMapUpdater MapUpdater { get; }
        MapDestructionAlgorithm MapDestructionAlgorithm { get; }
        void AddKill(NetworkConnectionToClient source, NetworkConnectionToClient receiver);

        void AddPlayer(NetworkConnectionToClient connection, GameClass chosenClass, CSteamID steamID,
            string nickname);

        void ChangeClass(NetworkConnectionToClient connection, GameClass chosenClass);
        void DeletePlayer(NetworkConnectionToClient connection);
        void CreateSpawnPoints();
    }
}