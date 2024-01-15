using Data;
using Mirror;
using Networking.Messages.Requests;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.Drill
{
    public class DrillView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly GameObject _drillInfo;
        private readonly TextMeshProUGUI _drillCountText;
        private readonly Sprite _drillCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private bool _isSelected;

        public DrillView(DrillItem configuration, Hud hud)
        {
            Icon = configuration.inventoryIcon;
            _drillInfo = hud.ItemInfo;
            _drillCountText = hud.ItemCount;
            _drillCountIcon = configuration.countIcon;
            _itemType = hud.ItemIcon;
        }

        public void Enable()
        {
            _drillInfo.SetActive(true);
            _itemType.sprite = _drillCountIcon;
        }

        public void UpdateAmmoText(string ammoText)
        {
            _drillCountText.SetText(ammoText);
        }

        public void Disable()
        {
            _drillInfo.SetActive(false);
        }
    }
}