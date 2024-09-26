using System.Collections.Generic;
using UnityEngine;

namespace Common.StaticData
{
    [CreateAssetMenu(fileName = "New inventory", menuName =  "Inventory System/Game Class Inventory")]
    public class GameInventory : ScriptableObject
    {
        public GameClass gameClass;
        public List<InventoryItem> inventory;
    }
}   