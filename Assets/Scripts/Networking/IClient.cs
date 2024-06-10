using System;
using System.Collections.Generic;
using Data;
using MapLogic;
using Networking.ClientServices;
using PlayerLogic;

namespace Networking
{
    public interface IClient
    {
        event Action<BlockDataWithPosition[]> MapUpdated;
        event Action<ServerTime> GameTimeChanged;
        event Action<ServerTime> RespawnTimeChanged;
        event Action<List<ScoreData>> ScoreboardChanged;
        event Action GameFinished;
        event Action<Player> PlayerCreated;
        MapLoadingProgress MapLoadingProgress { get; set; }
        IMapProvider MapProvider { get; set; }
        VerticalMapProjector MapProjector { get; set; }
        ClientPrefabRegistrar PrefabRegistrar { get; }
        void Start();
        void Stop();
    }
}