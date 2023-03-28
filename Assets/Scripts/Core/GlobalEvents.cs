using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public static class GlobalEvents
    {
        public static readonly UnityEvent<List<Vector3Int>, BlockData[]> OnBlockChangeStateEvent = new();

        public static void SendBlockStates(List<Vector3Int> positions, BlockData[] blocks)
        {
            OnBlockChangeStateEvent.Invoke(positions, blocks);
        }

        public static readonly UnityEvent<Color32> OnPaletteUpdate = new();

        public static void SendPaletteUpdate(Color32 color)
        {
            OnPaletteUpdate.Invoke(color);
        }

        public static readonly UnityEvent<string> OnSaveMapEvent = new();

        public static void SendMapSave(string saveName)
        {
            OnSaveMapEvent.Invoke(saveName);
        }

        public static readonly UnityEvent OnMapLoaded = new();

        public static void SendMapLoadedState()
        {
            OnMapLoaded.Invoke();
        }
    }
}