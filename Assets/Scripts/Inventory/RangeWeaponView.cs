using Data;
using Infrastructure.Services.Input;
using Mirror;
using Networking.Messages.Requests;
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
        private readonly Image _ammoType;
        private readonly Sprite _ammoTypeIcon;
        private readonly float _zoomMultiplier;


        public RangeWeaponView(IInputService inputService, Camera camera, Hud hud,
            RangeWeaponData configuration)
        {
            _inputService = inputService;
            Icon = configuration.InventoryIcon;
            _ammoTypeIcon = configuration.AmmoTypeIcon;
            BulletsInMagazine = configuration.BulletsInMagazine;
            TotalBullets = configuration.TotalBullets;
            _zoomMultiplier = configuration.ZoomMultiplier;
            _fpsCam = camera;
            _ammoInfo = hud.ammoInfo;
            _ammoCount = hud.ammoCount.GetComponent<TextMeshProUGUI>();
            _ammoType = hud.ammoType;
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
            {
                NetworkClient.Send(new ReloadRequest());
            }
        }

        public void OnLeftMouseButtonDown()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            NetworkClient.Send(new ShootRequest(ray, false));
        }

        public void OnLeftMouseButtonHold()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            NetworkClient.Send(new ShootRequest(ray, false));
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