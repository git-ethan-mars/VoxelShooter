using Common;
using UnityEngine;
using VoxelMap;

namespace GamePlay.Data
{
    public abstract class ItemConfigure : ScriptableObject
    {
        public int id;

        [Header("Visual/UI")]
        public Sprite inventoryIcon;

        public abstract InventoryItem CreateModel(IInventoryFactory inventoryFactory, RayCaster rayCaster, MapProvider mapProvider);
    }
}