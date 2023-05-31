using Infrastructure.Factory;
using Infrastructure.Services;
using Infrastructure.Services.StaticData;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class Inventory : NetworkBehaviour
    {
        public readonly SyncList<int> ItemIds = new();

        [HideInInspector] [SyncVar(hook = nameof(OnChangeSlot))]
        public int currentSlotId;

        private IStaticDataService _staticData;
        private GameObject _currentItemModel;
        private IInventoryModelFactory _inventoryModelFactory;

        private void Awake()
        {
            _staticData = AllServices.Container.Single<IStaticDataService>();
            _inventoryModelFactory = AllServices.Container.Single<IInventoryModelFactory>();
        }

        public override void OnStartClient()
        {
            OnChangeSlot(0, currentSlotId);
        }

        private void OnChangeSlot(int oldSlotId, int newSlotId)
        {
            if (_currentItemModel != null)
                Destroy(_currentItemModel);
            _currentItemModel = _inventoryModelFactory.CreateGameModel(_staticData.GetItem(ItemIds[newSlotId]).prefab,
                GetComponent<Player>().itemPosition);
        }
    }
}