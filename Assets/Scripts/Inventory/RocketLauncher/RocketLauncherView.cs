using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.RocketLauncher
{
    public class RocketLauncherView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly GameObject _rocketLauncherInfo;
        private readonly TextMeshProUGUI _rocketLauncherCountText;
        private readonly Sprite _rocketLauncherCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private bool _isSelected;

        public RocketLauncherView(RocketLauncherItem configuration, Hud hud)
        {
            Icon = configuration.inventoryIcon;
            _rocketLauncherInfo = hud.ItemInfo;
            _rocketLauncherCountText = hud.ItemCount;
            _rocketLauncherCountIcon = configuration.countIcon;
            _itemType = hud.ItemIcon;
        }

        public void Enable()
        {
            _rocketLauncherInfo.SetActive(true);
            _itemType.sprite = _rocketLauncherCountIcon;
        }

        public void UpdateAmmoText(string ammoText)
        {
            _rocketLauncherCountText.SetText(ammoText);
        }

        public void Disable()
        {
            _rocketLauncherInfo.SetActive(false);
        }
    }
}