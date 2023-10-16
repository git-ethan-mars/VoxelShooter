using Data;
using Mirror;
using Networking.Messages.Requests;
using Rendering;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class DrillView : IInventoryItemView, IConsumable, ILeftMouseButtonDownHandler
    {
        public Sprite Icon { get; }
        public int Count { get; set; }
        private readonly Raycaster _raycaster;
        private readonly float _delayInSeconds;
        private readonly int _itemId;
        private readonly GameObject _drillInfo;
        private readonly TextMeshProUGUI _drillCountText;
        private readonly Sprite _drillCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;

        public DrillView(Raycaster raycaster, DrillItem configuration, Hud hud)
        {
            Icon = configuration.inventoryIcon;
            _itemId = configuration.id;
            _drillInfo = hud.itemInfo;
            _drillCountText = hud.itemCount;
            _drillCountIcon = configuration.countIcon;
            _itemType = hud.itemIcon;
            Count = configuration.count;
            _raycaster = raycaster;
        }
        

        public void OnCountChanged()
        {
            _drillCountText.SetText(Count.ToString());
        }

        public void Select()
        {
            _drillInfo.SetActive(true);
            _itemType.sprite = _drillCountIcon;
            _drillCountText.SetText(Count.ToString());
        }
        
        public void Unselect()
        {
            _drillInfo.SetActive(false);
        }

        public void OnLeftMouseButtonDown()
        {
            Debug.Log(_itemId);
            NetworkClient.Send(new DrillSpawnRequest(_itemId,
                _raycaster.CentredRay));
        }
    }
}