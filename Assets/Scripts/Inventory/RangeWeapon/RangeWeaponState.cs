using CameraLogic;
using Data;
using Infrastructure.Services.Storage;
using PlayerLogic;
using UI;
using UnityEngine;

namespace Inventory.RangeWeapon
{
    public class RangeWeaponState : IInventoryItemState
    {
        public IInventoryItemModel ItemModel => _rangeWeaponModel;
        public IInventoryItemView ItemView => _rangeWeaponView;
        private readonly IInventoryInput _inventoryInput;
        private readonly RangeWeaponModel _rangeWeaponModel;
        private readonly RangeWeaponView _rangeWeaponView;


        public RangeWeaponState(IInventoryInput inventoryInput, IStorageService storageService, RayCaster rayCaster,
            RangeWeaponItem configure, RangeWeaponData data, Camera camera, Player player, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _rangeWeaponModel = new RangeWeaponModel(storageService, rayCaster, camera, configure, data, player);
            _rangeWeaponView = new RangeWeaponView(configure, data, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _rangeWeaponModel.ShootSingle;
            _inventoryInput.FirstActionButtonHold += _rangeWeaponModel.ShootAutomatic;
            _inventoryInput.FirstActionButtonUp += _rangeWeaponModel.CancelShoot;
            _inventoryInput.SecondActionButtonDown += _rangeWeaponModel.ZoomIn;
            _inventoryInput.SecondActionButtonUp += _rangeWeaponModel.ZoomOut;
            _inventoryInput.ReloadButtonDown += _rangeWeaponModel.Reload;
            _rangeWeaponModel.BulletsInMagazine.ValueChanged += _rangeWeaponView.OnBulletsInMagazineChanged;
            _rangeWeaponModel.TotalBullets.ValueChanged += _rangeWeaponView.OnTotalBulletsChanged;
            _rangeWeaponView.Enable();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _rangeWeaponModel.ShootSingle;
            _inventoryInput.FirstActionButtonHold -= _rangeWeaponModel.ShootAutomatic;
            _inventoryInput.FirstActionButtonUp -= _rangeWeaponModel.CancelShoot;
            _inventoryInput.SecondActionButtonDown -= _rangeWeaponModel.ZoomIn;
            _inventoryInput.SecondActionButtonUp -= _rangeWeaponModel.ZoomOut;
            _inventoryInput.ReloadButtonDown -= _rangeWeaponModel.Reload;
            _rangeWeaponModel.BulletsInMagazine.ValueChanged -= _rangeWeaponView.OnBulletsInMagazineChanged;
            _rangeWeaponModel.TotalBullets.ValueChanged -= _rangeWeaponView.OnTotalBulletsChanged;
            _rangeWeaponModel.CancelShoot();
            _rangeWeaponModel.ZoomOut();
            _rangeWeaponView.Disable();
        }
    }
}