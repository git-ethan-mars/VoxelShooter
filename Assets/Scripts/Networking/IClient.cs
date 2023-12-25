using System;
using System.Collections.Generic;
using Data;
using MapLogic;
using PlayerLogic;

namespace Networking
{
    public interface IClient
    {
        event Action MapDownloaded;
        event Action<BlockDataWithPosition[]> MapUpdated;
        event Action<float> MapLoadProgressed;
        event Action<ServerTime> GameTimeChanged;
        event Action<ServerTime> RespawnTimeChanged;
        event Action<List<ScoreData>> ScoreboardChanged;
        event Action GameFinished;
        event Action<Player> PlayerCreated;
        MapProvider MapProvider { get; set; }
        string MapName { get; set; }
        void Start();
        void Stop();
    }
}