using GamePlay;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu(menuName = "Inventory items", fileName = "New item")]
    public class InventoryItem : ScriptableObject
    {
        public Sprite icon;
        public string name;
        public string description;
        public BlockKind kind;
    }
}