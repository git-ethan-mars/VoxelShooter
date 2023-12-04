using CustomAttributes;
using UnityEngine;

namespace Data
{
    public abstract class InventoryItem : ScriptableObject
    {
        public int id;

        [ReadOnly]
        public ItemType itemType;

        [Header("Visual/UI")]
        public Sprite inventoryIcon;

        public GameObject prefab;
    }
}