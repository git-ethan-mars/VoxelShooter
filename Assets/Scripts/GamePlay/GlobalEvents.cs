using UnityEngine;
using UnityEngine.Events;

namespace GamePlay
{
    public static class GlobalEvents
    {
        public static readonly UnityEvent<Block, Vector3Int> onBlockChangeStateEvent = new();

        public static void SendBlockState(Block block, Vector3Int position)
        {
            onBlockChangeStateEvent.Invoke(block, position);
        }

        public static readonly UnityEvent<byte> onPaletteUpdate = new();

        public static void SendPaletteUpdate(byte colorId)
        {
            onPaletteUpdate.Invoke(colorId);
        }

        public static readonly UnityEvent<string> OnSaveMapEvent = new();

        public static void SendMapSave(string saveName)
        {
            OnSaveMapEvent.Invoke(saveName);
        }
    }
}