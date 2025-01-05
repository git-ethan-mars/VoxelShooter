using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GamePlay.Data
{
    [CreateAssetMenu(fileName = "New inventory", menuName =  "Inventory System/Game Class Inventory")]
    public class GameInventory : ScriptableObject
    {
        public GameClass gameClass;
        [FormerlySerializedAs("inventory")] public List<ItemConfigure> items;
    }
}   