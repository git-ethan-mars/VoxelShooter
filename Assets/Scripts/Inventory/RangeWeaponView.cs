using Data;
using Infrastructure.Services.Input;
using Networking.Synchronization;
using PlayerLogic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class RangeWeaponView : IInventoryItemView, IReloading, IShooting, ILeftMouseButtonDownHandler, 
        ILeftMouseButtonHoldHandler, IRightMouseButtonDownHandler, IRightMouseButtonUpHandler, IUpdated
    {
        public Sprite Icon { get; }
        public int BulletsInMagazine { get; set; }
        public int TotalBullets { get; set; }
        private readonly IInputService _inputService;
        private readonly GameObject _ammoInfo;
        private readonly TextMeshProUGUI _ammoCount;
        private readonly Camera _fpsCam;
        private readonly RangeWeaponSynchronization _bulletSynchronization;
        private readonly Image _ammoType;
        private readonly Sprite _ammoTypeIcon;
        private readonly int _id;
        private readonly float _zoomMultiplier;


        public RangeWeaponView(IInputService inputService, Camera camera, Player player, Hud hud, RangeWeaponData configuration)
        {
            _inputService = inputService;
            Icon = configuration.InventoryIcon;
            _ammoTypeIcon = configuration.AmmoTypeIcon;
            _id = configuration.ID;
            BulletsInMagazine = configuration.BulletsInMagazine;
            TotalBullets = configuration.TotalBullets;
            _zoomMultiplier = configuration.ZoomMultiplier;
            _fpsCam = camera;
            _ammoInfo = hud.ammoInfo;
            _ammoCount = hud.ammoCount.GetComponent<TextMeshProUGUI>();
            _ammoType = hud.ammoType;
            _bulletSynchronization = player.GetComponent<RangeWeaponSynchronization>();
        }


        public void Select()
        {
            _ammoInfo.SetActive(true);
            _ammoType.sprite = _ammoTypeIcon;
            _ammoCount.SetText($"{BulletsInMagazine} / {TotalBullets}");
        }


        public void Unselect()
        {
            _ammoInfo.SetActive(false);
            _fpsCam.fieldOfView = Constants.DefaultFov;
        }

        public void InnerUpdate()
        {
            if (_inputService.IsReloadingButtonDown())
                _bulletSynchronization.CmdReload(_id);
        }

        public void OnLeftMouseButtonDown()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _bulletSynchronization.CmdShootSingle(ray, _id);
        }

        public void OnLeftMouseButtonHold()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _bulletSynchronization.CmdShootAutomatic(ray, _id);
        }

        public void OnRightMouseButtonDown()
        {
            _fpsCam.fieldOfView = Constants.DefaultFov / _zoomMultiplier;
        }
        
        public void OnRightMouseButtonUp()
        {
            _fpsCam.fieldOfView = Constants.DefaultFov;
        }

        public void OnReloadResult()
        {
            _ammoCount.SetText($"{BulletsInMagazine} / {TotalBullets}");
        }

        public void OnShootResult()
        {
            _ammoCount.SetText($"{BulletsInMagazine} / {TotalBullets}");
        }
    }
}