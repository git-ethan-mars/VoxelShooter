using Data;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Networking.Synchronization;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class RangeWeaponView : IInventoryItemView, ILeftMouseButtonDownHandler, ILeftMouseButtonHoldHandler, IUpdated
    {
        public Sprite Icon { get; }
        private GameObject Model { get; }
        private readonly RangeWeaponData _rangeWeapon;
        private readonly GameObject _ammoInfo;
        private readonly TextMeshProUGUI _ammoCount;
        private readonly Camera _fpsCam;
        private readonly RangeWeaponSynchronization _bulletSynchronization;
        private readonly IInputService _inputService;
        private readonly Image _ammoType;


        public RangeWeaponView(IGameFactory gameFactory, IInputService inputService, Camera camera,
            Transform itemPosition, GameObject player, GameObject hud, RangeWeaponData configuration)
        {
            _inputService = inputService;
            _rangeWeapon = configuration;
            Model = gameFactory.CreateGameModel(_rangeWeapon.Prefab, itemPosition);
            Model.SetActive(false);
            Icon = _rangeWeapon.InventoryIcon;
            Model.GetComponentInChildren<Transform>();
            _fpsCam = camera;
            _ammoInfo = hud.GetComponent<Hud>().ammoInfo;
            _ammoCount = hud.GetComponent<Hud>().ammoCount.GetComponent<TextMeshProUGUI>();
            _ammoType = hud.GetComponent<Hud>().ammoType;
            _bulletSynchronization = player.GetComponent<RangeWeaponSynchronization>();
        }

        public void Select()
        {
            _ammoInfo.SetActive(true);
            _ammoType.sprite = _rangeWeapon.AmmoTypeIcon;
            Model.SetActive(true);
        }


        public void Unselect()
        {
            _ammoInfo.SetActive(false);
            Model.SetActive(false);
        }

        public void InnerUpdate()
        {
            if (_inputService.IsReloadingButtonDown())
                _bulletSynchronization.CmdReload(_rangeWeapon.ID);
            _ammoCount.SetText($"{_rangeWeapon.BulletsInMagazine} / {_rangeWeapon.TotalBullets}");
        }

        public void OnLeftMouseButtonDown()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _bulletSynchronization.CmdShootSingle(ray, _rangeWeapon.ID);
        }

        public void OnLeftMouseButtonHold()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _bulletSynchronization.CmdShootAutomatic(ray, _rangeWeapon.ID);
        }
    }
}