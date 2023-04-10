using UnityEngine;

namespace Data
{
    public abstract class InventoryItem : ScriptableObject
    {
        public int id;
        public ItemType itemType;
        [Header("Visual/UI")]
        public Sprite icon;
        [Header("Model")]
        public GameObject prefab;
    }
    public enum ItemType
    {
        PrimaryWeapon,
        SecondaryWeapon,
        MeleeWeapon,
        Dynamite,
        Block,
        Brush,
        SpawnPoint,
    }
}