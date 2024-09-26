using UnityEngine;

namespace Inventory
{
    public interface IInventoryItemView
    {
        public Sprite Icon { get; }
        void Enable();
        void Disable();
    }
}