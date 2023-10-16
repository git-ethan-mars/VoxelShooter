using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    public abstract class InventoryItem : ScriptableObject
    {
        public int id;
        public ItemType itemType;
        [FormerlySerializedAs("icon")] [Header("Visual/UI")]
        public Sprite inventoryIcon;
        public GameObject prefab;
    }
    public enum ItemType
    {
        RangeWeapon,
        MeleeWeapon,
        Tnt,
        Block,
        Brush,
        Grenade,
        RocketLauncher,
        Drill
    }
}