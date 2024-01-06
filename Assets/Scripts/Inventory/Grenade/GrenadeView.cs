using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.Grenade
{
    public class GrenadeView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly GameObject _grenadeInfo;
        private readonly TextMeshProUGUI _grenadeCountText;
        private readonly Sprite _grenadeCountIcon;
        private readonly Sprite _itemTypeIcon;
        private readonly Image _itemType;
        private bool _isSelected;

        public GrenadeView(GrenadeItem configure, Hud hud)
        {
            Icon = configure.inventoryIcon;
            _grenadeInfo = hud.ItemInfo;
            _grenadeCountText = hud.ItemCount;
            _grenadeCountIcon = configure.countIcon;
            _itemType = hud.ItemIcon;
        }

        public void Enable()
        {
            _grenadeInfo.SetActive(true);
            _itemType.sprite = _grenadeCountIcon;
        }

        public void Disable()
        {
            _grenadeInfo.SetActive(false);
        }

        public void UpdateAmmoText(string ammoText)
        {
            _grenadeCountText.SetText(ammoText);
        }
    }
}