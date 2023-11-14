using System;
using System.Collections.Generic;
using Data;
using Generators;
using Infrastructure.Services.StaticData;
using MapLogic;
using Networking.ClientServices;

namespace Networking
{
    public interface IClient
    {
        event Action<float> MapLoadProgressed;
        event Action<ServerTime> GameTimeChanged;
        event Action<ServerTime> RespawnTimeChanged;
        event Action<List<ScoreData>> ScoreboardChanged;
        event Action GameFinished;
        FallMeshGenerator FallMeshGenerator { get; }
        IStaticDataService StaticData { get; }
        ClientData Data { get; }
        MapProvider MapProvider { get; set; }
        MapGenerator MapGenerator { get; }
        void Start();
        void Stop();
    }
}