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

        public static readonly UnityEvent<Block> OnBlockChoiceEvent = new();

        public static void SendBlockChoice(Block block)
        {
            OnBlockChoiceEvent.Invoke(block);
        }
    }
}