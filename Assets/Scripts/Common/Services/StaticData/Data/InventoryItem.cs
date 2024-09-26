using UnityEngine;

namespace Common.StaticData
{
    public abstract class InventoryItem : ScriptableObject
    {
        public int id;

        [Header("Visual/UI")]
        public Sprite inventoryIcon;

        public GameObject prefab;
    }
}