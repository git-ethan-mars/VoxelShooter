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
    public class RocketLauncherView : IInventoryItemView, IConsumable, ILeftMouseButtonDownHandler
    {
        public Sprite Icon { get; }
        public int Count { get; set; }
        private readonly Raycaster _raycaster;
        private readonly float _delayInSeconds;
        private readonly int _itemId;
        private readonly GameObject _rocketLauncherInfo;
        private readonly TextMeshProUGUI _rocketLauncherCountText;
        private readonly Sprite _rocketLauncherCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;

        public RocketLauncherView(Raycaster raycaster, RocketLauncherItem configuration, Hud hud)
        {
            Icon = configuration.inventoryIcon;
            _itemId = configuration.id;
            _rocketLauncherInfo = hud.itemInfo;
            _rocketLauncherCountText = hud.itemCount;
            _rocketLauncherCountIcon = configuration.countIcon;
            _itemType = hud.itemIcon;
            Count = configuration.count;
            _raycaster = raycaster;
        }
        

        public void OnCountChanged()
        {
            _rocketLauncherCountText.SetText(Count.ToString());
        }

        public void Select()
        {
            _rocketLauncherInfo.SetActive(true);
            _itemType.sprite = _rocketLauncherCountIcon;
            _rocketLauncherCountText.SetText(Count.ToString());
        }


        public void Unselect()
        {
            _rocketLauncherInfo.SetActive(false);
        }

        public void OnLeftMouseButtonDown()
        {
            NetworkClient.Send(new RocketSpawnRequest(_itemId,
                _raycaster.CentredRay));
        }
    }
}