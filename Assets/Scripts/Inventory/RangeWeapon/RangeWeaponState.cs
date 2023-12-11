using CameraLogic;
using Data;
using PlayerLogic;
using UI;

namespace Inventory.RangeWeapon
{
    public class RangeWeaponState : IInventoryItemState
    {
        public IInventoryItemModel ItemModel => _rangeWeaponModel;
        public IInventoryItemView ItemView => _rangeWeaponView;
        private readonly RangeWeaponModel _rangeWeaponModel;
        private readonly RangeWeaponView _rangeWeaponView;
        private readonly IInventoryInput _inventoryInput;


        public RangeWeaponState(IInventoryInput inventoryInput, RayCaster rayCaster,
            RangeWeaponItem configure, RangeWeaponData data, Player player, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _rangeWeaponModel = new RangeWeaponModel(rayCaster, configure, data, player);
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
            _rangeWeaponModel.ZoomOut();
            _rangeWeaponView.Disable();
        }
    }
}