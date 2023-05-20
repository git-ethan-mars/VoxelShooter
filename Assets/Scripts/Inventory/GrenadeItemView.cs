using Data;
using UnityEngine;

namespace Inventory
{
    public class GrenadeItemView : IInventoryItemView, ILeftMouseButtonDownHandler, IConsumable
    {
        public Sprite Icon { get; }
        public int Count { get; set; }
        
        public GrenadeItemView(GrenadeItem config)
        {
            Icon = config.inventoryIcon;
        }
        
        public void Select()
        {
        }

        public void Unselect()
        {
        }

        public void OnLeftMouseButtonDown()
        {
        }

        public void OnCountChanged()
        {
        }
    }
}