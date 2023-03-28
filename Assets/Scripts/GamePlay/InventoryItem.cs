using UnityEngine;

namespace GamePlay
{
    public abstract class InventoryItem : ScriptableObject
    {
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
    }
}