using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class RocketLauncherView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly GameObject _rocketLauncherInfo;
        private readonly TextMeshProUGUI _rocketLauncherCountText;
        private readonly Sprite _rocketLauncherCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private int _count;

        public RocketLauncherView(RocketLauncherItem configuration, Hud hud)
        {
            Icon = configuration.inventoryIcon;
            _rocketLauncherInfo = hud.itemInfo;
            _rocketLauncherCountText = hud.itemCount;
            _rocketLauncherCountIcon = configuration.countIcon;
            _itemType = hud.itemIcon;
            _count = configuration.count;
        }

        public void Enable()
        {
            _rocketLauncherInfo.SetActive(true);
            _itemType.sprite = _rocketLauncherCountIcon;
            _rocketLauncherCountText.SetText(_count.ToString());
        }

        public void Disable()
        {
            _rocketLauncherInfo.SetActive(false);
        }

        public void OnCountChanged(int count)
        {
            _count = count;
            _rocketLauncherCountText.SetText(_count.ToString());
        }
    }
}