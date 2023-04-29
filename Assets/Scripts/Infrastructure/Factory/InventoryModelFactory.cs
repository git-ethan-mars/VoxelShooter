using UnityEngine;

namespace Infrastructure.Factory
{
    public class InventoryModelFactory : IInventoryModelFactory
    {
        public GameObject CreateGameModel(GameObject model, Transform itemPosition) => Object.Instantiate(model, itemPosition);
    }
}