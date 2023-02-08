using UI;
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

        public static readonly UnityEvent<InventoryItem> OnSlotChangeEvent = new();

        public static void SendSlotChoice(InventoryItem item)
        {
            OnSlotChangeEvent.Invoke(item);
        }

        public static readonly UnityEvent<Color> OnColorBlockChange = new();

        public static void SendColorBlockChange(Color color)
        {
            OnColorBlockChange.Invoke(color);
        }
    }
}