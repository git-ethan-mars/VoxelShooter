using UnityEngine;

namespace GamePlay.Data
{
    public interface IItemData
    {
	    public int ID { get; }
        Sprite InventoryIcon { get; }
    }
}