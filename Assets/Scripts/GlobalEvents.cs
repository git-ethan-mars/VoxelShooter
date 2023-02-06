using UnityEngine;
using UnityEngine.Events;

public static class GlobalEvents
{
    public static readonly UnityEvent<Block, Vector3Int> OnBlockChangeStateEvent = new();

    public static void OnBlockChangeState(Block block, Vector3Int position)
    {
        OnBlockChangeStateEvent.Invoke(block, position);
    }
}