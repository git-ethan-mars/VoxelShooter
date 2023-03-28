using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InventoryView : MonoBehaviour
    {
        public GameObject[] Boarders => slots.Select(slot => slot.transform.Find("Boarder").gameObject).ToArray();

        [SerializeField] private List<GameObject> slots;
        public int SlotsCount => slots.Count;
        

        public void SetIconForItem(int slotIndex, Sprite icon)
        {
            slots[slotIndex].GetComponent<Image>().sprite = icon;
        }
        
    }
    
    
}