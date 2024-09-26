using System;
using System.Collections.Generic;
using Common.StaticData;
using UnityEngine;
using VoxelMap;

namespace Networking.Client
{
    public interface IClient
    {
        event Action<GameObject> LocalPlayerCreated;
        event Action<string, MapData> MapLoaded;
        event Action<float> MapLoadProgressed;
        event Action<ServerTime> GameTimeChanged;
        event Action<ServerTime> RespawnTimeChanged;
        event Action<List<ScoreData>> ScoreboardChanged;
        event Action GameFinished;
        void ChangeClass(GameClass gameClass);
        
        void Start();
        void Stop();
    }
}