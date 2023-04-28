using Infrastructure.Services;
using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IInventoryModelFactory : IService
    {
        public GameObject CreateGameModel(GameObject model, Transform itemPosition);
    }
}