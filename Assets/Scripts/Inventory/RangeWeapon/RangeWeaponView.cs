using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class RangeWeaponView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private int _totalBullets;
        private readonly GameObject _ammoInfo;
        private readonly TextMeshProUGUI _ammoCount;
        private readonly Camera _fpsCam;
        private readonly Image _ammoType;
        private readonly Sprite _ammoTypeIcon;
        private int _bulletsInMagazine;
        private bool _isSelected;

        public RangeWeaponView(Camera camera, RangeWeaponData configure,
            Hud hud)
        {
            Icon = configure.InventoryIcon;
            _ammoTypeIcon = configure.AmmoTypeIcon;
            _bulletsInMagazine = configure.BulletsInMagazine;
            _totalBullets = configure.TotalBullets;
            _fpsCam = camera;
            _ammoInfo = hud.ammoInfo;
            _ammoCount = hud.ammoCount;
            _ammoType = hud.ammoType;
        }

        public void Enable()
        {
            _ammoInfo.SetActive(true);
            _isSelected = true;
            _ammoType.sprite = _ammoTypeIcon;
            _ammoCount.SetText($"{_bulletsInMagazine} / {_totalBullets}");
        }

        public void Disable()
        {
            _ammoInfo.SetActive(false);
            _isSelected = false;
            _fpsCam.fieldOfView = Constants.DefaultFov;
        }

        public void OnTotalBulletsChanged(int totalBullets)
        {
            _totalBullets = totalBullets;
            if (_isSelected)
                _ammoCount.SetText($"{_bulletsInMagazine} / {_totalBullets}");
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