using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory.RangeWeapon
{
    public class RangeWeaponView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly GameObject _ammoInfo;
        private readonly TextMeshProUGUI _ammoCount;
        private readonly Image _ammoType;
        private readonly Sprite _ammoTypeIcon;

        public RangeWeaponView(RangeWeaponItem configure, Hud hud)
        {
            Icon = configure.inventoryIcon;
            _ammoTypeIcon = configure.ammoIcon;
            _ammoInfo = hud.ammoInfo;
            _ammoCount = hud.ammoCount;
            _ammoType = hud.ammoType;
        }

        public void Enable()
        {
            _ammoInfo.SetActive(true);
            _ammoType.sprite = _ammoTypeIcon;
        }

        public void UpdateAmmoText(string ammoText)
        {
            _ammoCount.SetText(ammoText);
        }

        public void Disable()
        {
            _ammoInfo.SetActive(false);
        }
    }
}