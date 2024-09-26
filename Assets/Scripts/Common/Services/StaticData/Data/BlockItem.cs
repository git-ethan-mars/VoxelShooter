using UnityEngine;

namespace Common.StaticData
{
    [CreateAssetMenu(fileName = "Block", menuName = "Inventory System/Inventory Items/Block")]
    public class BlockItem : InventoryItem
    {
        public Sprite itemSprite;
        [Header("Configuration")]
        public int count;
        public void Awake()
        {
        }
    }
}