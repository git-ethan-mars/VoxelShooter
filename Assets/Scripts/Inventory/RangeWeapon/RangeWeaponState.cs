using Data;
using UI;
using UnityEngine;

namespace Inventory
{
    public class RangeWeaponState : IInventoryItemState
    {
        public IInventoryItemModel ItemModel => _rangeWeaponModel;
        public IInventoryItemView ItemView => _rangeWeaponView;
        private readonly IInventoryInput _inventoryInput;
        private readonly RangeWeaponModel _rangeWeaponModel;
        private readonly RangeWeaponView _rangeWeaponView;


        public RangeWeaponState(IInventoryInput inventoryInput, RayCaster rayCaster, RangeWeaponData configure,
            Camera camera, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _rangeWeaponModel = new RangeWeaponModel(rayCaster, configure);
            _rangeWeaponView = new RangeWeaponView(camera, configure, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _rangeWeaponModel.ShootSingle;
            _inventoryInput.FirstActionButtonHold += _rangeWeaponModel.ShootAutomatic;
            _inventoryInput.ReloadButtonDown += _rangeWeaponModel.Reload;
            _inventoryInput.SecondActionButtonDown += _rangeWeaponModel.ZoomIn;
            _inventoryInput.SecondActionButtonUp += _rangeWeaponModel.ZoomOut;
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
            _inventoryInput.ReloadButtonDown -= _rangeWeaponModel.Reload;
            _inventoryInput.SecondActionButtonDown -= _rangeWeaponModel.ZoomIn;
            _inventoryInput.SecondActionButtonUp -= _rangeWeaponModel.ZoomOut;
            _rangeWeaponModel.BulletsInMagazine.ValueChanged -= _rangeWeaponView.OnBulletsInMagazineChanged;
            _rangeWeaponModel.TotalBullets.ValueChanged -= _rangeWeaponView.OnTotalBulletsChanged;
            _rangeWeaponView.Disable();
        }
    }
}