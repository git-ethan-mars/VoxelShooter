using UnityEngine;

namespace Inventory
{
    public interface IInventoryItemView : ISelectable, IUnselectable
    {
        public Sprite Icon { get; }
    }
}