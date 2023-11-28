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
        private int _totalBullets;
        private readonly GameObject _ammoInfo;
        private readonly TextMeshProUGUI _ammoCount;
        private readonly Image _ammoType;
        private readonly Sprite _ammoTypeIcon;
        private int _bulletsInMagazine;
        private bool _isSelected;

        public RangeWeaponView(RangeWeaponData configure,
            Hud hud)
        {
            Icon = configure.InventoryIcon;
            _ammoTypeIcon = configure.AmmoTypeIcon;
            _bulletsInMagazine = configure.BulletsInMagazine;
            _totalBullets = configure.TotalBullets;
            _ammoInfo = hud.ammoInfo;
            _ammoCount = hud.ammoCount;
            _ammoType = hud.ammoType;
        }

        public void Enable()
        {
            _isSelected = true;
            _ammoInfo.SetActive(true);
            _ammoType.sprite = _ammoTypeIcon;
            _ammoCount.SetText($"{_bulletsInMagazine} / {_totalBullets}");
        }

        public void Disable()
        {
            _isSelected = false;
            _ammoInfo.SetActive(false);
        }

        public void OnTotalBulletsChanged(int totalBullets)
        {
            _totalBullets = totalBullets;
            if (_isSelected)
            {
                _ammoCount.SetText($"{_bulletsInMagazine} / {_totalBullets}");
            }
        }

        public void OnBulletsInMagazineChanged(int bulletsInMagazine)
        {
            _bulletsInMagazine = bulletsInMagazine;
            if (_isSelected)
            {
                _ammoCount.SetText($"{_bulletsInMagazine} / {_totalBullets}");
            }
        }
    }
}