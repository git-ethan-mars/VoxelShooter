using UnityEngine;
using UnityEngine.Events;

namespace GamePlay
{
    public static class GlobalEvents
    {
        public static readonly UnityEvent<Block, Vector3Int> OnBlockChangeStateEvent = new();

        public static void SendBlockState(Block block, Vector3Int position)
        {
            OnBlockChangeStateEvent.Invoke(block, position);
        }

        public static readonly UnityEvent<byte> OnPaletteUpdate = new();

        public static void SendPaletteUpdate(byte colorId)
        {
            OnPaletteUpdate.Invoke(colorId);
        }

        public static readonly UnityEvent<string> OnSaveMapEvent = new();

        public static void SendMapSave(string saveName)
        {
            OnSaveMapEvent.Invoke(saveName);
        }
    }
}