using GamePlay;
using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    public static class GlobalEvents
    {
        public static readonly UnityEvent<Block, Vector3Int> OnBlockChangeStateEvent = new();

        public static void SendBlockState(Block block, Vector3Int position)
        {
            OnBlockChangeStateEvent.Invoke(block, position);
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
    }
}