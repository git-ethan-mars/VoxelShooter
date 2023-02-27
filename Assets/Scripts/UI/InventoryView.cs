using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventoryView : MonoBehaviour
    {
        
        [SerializeField] private List<GameObject> slots;
        
        public int SlotsCount => slots.Count;
        

        public void SetIconForItem(int slotIndex, Sprite icon)
        {
            slots[slotIndex].GetComponent<Image>().sprite = icon;
        }

        public void SetPointer(int slotIndex, IInventoryItemView inventoryItem)
        {
            inventoryItem.Pointer = slots[slotIndex].transform.Find("Boarder").gameObject;
        }
    }
    
    
}