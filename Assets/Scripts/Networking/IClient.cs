using System;
using System.Collections.Generic;
using Data;
using Generators;
using Infrastructure.Factory;
using MapLogic;
using Networking.ClientServices;

namespace Networking
{
    public interface IClient
    {
        event Action MapDownloaded;
        event Action<float> MapLoadProgressed;
        event Action<ServerTime> GameTimeChanged;
        event Action<ServerTime> RespawnTimeChanged;
        event Action<List<ScoreData>> ScoreboardChanged;
        ClientData Data { get; }
        IGameFactory GameFactory { get; }
        IMeshFactory MeshFactory { get; }
        IMapProvider MapProvider { get; set; }
        MapGenerator MapGenerator { get; set; }
        void RegisterHandlers();
        void UnregisterHandlers();
    }
}