using System;
using System.Collections.Generic;
using Data;
using Generators;
using MapLogic;
using Networking.ClientServices;
using PlayerLogic;

namespace Networking
{
    public interface IClient
    {
        event Action<float> MapLoadProgressed;
        event Action<ServerTime> GameTimeChanged;
        event Action<ServerTime> RespawnTimeChanged;
        event Action<List<ScoreData>> ScoreboardChanged;
        event Action GameFinished;
        ClientData Data { get; }
        MapProvider MapProvider { get; set; }
        ChunkMeshProvider ChunkMeshProvider { get; }
        void Start();
        void Stop();
        event Action<Player> PlayerCreated;
    }
}