using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private List<InventoryItem> inventoryItems;

        private void Start()
        {
            foreach (var inventoryItem in inventoryItems)
            {
                Instantiate(buttonPrefab, transform);
                var image = buttonPrefab.GetComponent<Image>();
                image.type = Image.Type.Simple;
                image.sprite = inventoryItem.icon;
            }
        }
    }
}