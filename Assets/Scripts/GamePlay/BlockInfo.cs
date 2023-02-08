using UnityEngine;

namespace GamePlay
{
    [CreateAssetMenu(menuName = "Test/Block info", fileName = "New block info")]
    public class BlockInfo : ScriptableObject
    {
        public Vector2 leftBottomUVPosition;
        public BlockKind kind;
    }
}