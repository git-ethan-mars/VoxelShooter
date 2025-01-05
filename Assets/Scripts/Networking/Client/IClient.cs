using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GamePlay;
using GamePlay.Data;
using VoxelMap;

namespace Networking.Client
{
    public interface IClient
    {
        event Action<string, MapData> MapLoaded;
        event Action<float> MapLoadProgressed;
        event Action<ServerTime> GameTimeChanged;
        event Action<ServerTime> RespawnTimeChanged;
        event Action<List<ScoreData>> ScoreboardChanged;
        event Action<Character> CharacterSpawned;
        event Action<Character> CharacterDespawned; 
        event Func<UniTask> GameFinished;
        void ChangeClass(GameClass gameClass);
        
        void Start();
        void Stop();
    }
}