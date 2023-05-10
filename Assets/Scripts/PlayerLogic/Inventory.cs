using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class Inventory : NetworkBehaviour
    {
        public readonly SyncList<int> ItemIds = new();
        [HideInInspector] [SyncVar(hook = nameof(OnChangeSlot))] public int currentSlotId;
        private IStaticDataService _staticData;
        private GameObject _currentItemModel;
        
        private void Awake()
        {
            _staticData = AllServices.Container.Single<IStaticDataService>();
        }

        private void OnChangeSlot(int oldSlotId, int newSlotId)
        {
            if (_currentItemModel is not null)
                Destroy(_currentItemModel);
            _currentItemModel = Instantiate(_staticData.GetItem(ItemIds[newSlotId]).prefab, GetComponent<Player>().itemPosition);
        }
    }
}