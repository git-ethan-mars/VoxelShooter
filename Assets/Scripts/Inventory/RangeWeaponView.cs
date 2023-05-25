using Data;
using Infrastructure.Factory;
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
        private GameObject Model { get; }
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


        public RangeWeaponView(IInventoryModelFactory gameFactory, IInputService inputService, Camera camera,
            Transform itemPosition, GameObject player, GameObject hud, RangeWeaponData configuration)
        {
            _inputService = inputService;
            Model = gameFactory.CreateGameModel(configuration.Prefab, itemPosition);
            Model.SetActive(false);
            Icon = configuration.InventoryIcon;
            _ammoTypeIcon = configuration.AmmoTypeIcon;
            _id = configuration.ID;
            BulletsInMagazine = configuration.BulletsInMagazine;
            TotalBullets = configuration.TotalBullets;
            _zoomMultiplier = configuration.ZoomMultiplier;
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
            _ammoType.sprite = _ammoTypeIcon;
            _ammoCount.SetText($"{BulletsInMagazine} / {TotalBullets}");
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