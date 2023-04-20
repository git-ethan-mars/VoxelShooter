using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Melee Weapon", menuName = "Inventory System/Inventory Items/Melee weapon")]
    public class MeleeWeaponItem : InventoryItem
    {
        
        private void Awake()
        {
            itemType = ItemType.MeleeWeapon;
        }
    }
}