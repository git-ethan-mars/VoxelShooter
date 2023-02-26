using System.Collections.Generic;
using GamePlay;
using Mirror;
using UnityEngine;

namespace UI
{
    public class InventoryController : NetworkBehaviour
    {
        public override void OnStartAuthority()
        {
            var itemList = new List<IInventoryItemView>
                {new BlockView(GetComponent<BlockPlacement>()), new BrushView(GetComponent<ColoringBrush>())};
            var inventory = GameObject.Find("Canvas/GamePlay/Inventory");
            inventory.SetActive(true);
            inventory.GetComponent<InventoryView>().ItemList = itemList;
        }
    }
}