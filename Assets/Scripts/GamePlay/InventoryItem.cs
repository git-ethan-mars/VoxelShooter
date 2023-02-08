using UnityEngine;

namespace GamePlay
{
    [CreateAssetMenu(menuName = "Test/Inventory items", fileName = "New item")]
    public class InventoryItem : ScriptableObject
    {
        public Sprite icon;
        public BlockKind kind;
        public Color color;
    }
}