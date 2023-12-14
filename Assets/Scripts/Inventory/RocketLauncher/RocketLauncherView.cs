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
        private readonly RocketLauncherData _rocketLauncherData;

        public RocketLauncherView(RocketLauncherItem configuration, Hud hud, RocketLauncherData rocketLauncherData)
        {
            Icon = configuration.inventoryIcon;
            _rocketLauncherInfo = hud.itemInfo;
            _rocketLauncherCountText = hud.itemCount;
            _rocketLauncherCountIcon = configuration.countIcon;
            _itemType = hud.itemIcon;
            _rocketLauncherData = rocketLauncherData;
        }

        public void Enable()
        {
            _rocketLauncherInfo.SetActive(true);
            _itemType.sprite = _rocketLauncherCountIcon;
            _rocketLauncherCountText.SetText($"{_rocketLauncherData.RocketsInSlotsCount}/{_rocketLauncherData.TotalRockets}");
        }

        public void Disable()
        {
            _rocketLauncherInfo.SetActive(false);
        }

        public void OnTotalRocketCountChanged(int count)
        {
            _rocketLauncherData.TotalRockets = count;
            _rocketLauncherCountText.SetText($"{_rocketLauncherData.RocketsInSlotsCount}/{count}");
        }
        
        public void OnRocketInSlotsCountChanged(int count)
        {
            _rocketLauncherData.RocketsInSlotsCount = count;
            _rocketLauncherCountText.SetText($"{count}/{_rocketLauncherData.TotalRockets}");
        }
    }
}