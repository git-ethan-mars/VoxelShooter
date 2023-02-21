using UnityEngine;

namespace UI
{
    public interface IInventoryItemView : ISelectable, IUnselectable
    {
        public Sprite Icon { get; }
        public GameObject Pointer { get; set; }
    }
}