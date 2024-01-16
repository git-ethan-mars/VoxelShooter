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
            RangeWeaponItem configure, RangeWeaponItemData itemData, Player player, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _rangeWeaponModel = new RangeWeaponModel(rayCaster, configure, itemData, player);
            _rangeWeaponView = new RangeWeaponView(configure, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _rangeWeaponModel.ShootSingle;
            _inventoryInput.FirstActionButtonHold += _rangeWeaponModel.ShootAutomatic;
            _inventoryInput.FirstActionButtonUp += _rangeWeaponModel.CancelShoot;
            _inventoryInput.SecondActionButtonDown += ZoomIn;
            _inventoryInput.SecondActionButtonUp += ZoomOut;
            _inventoryInput.ReloadButtonDown += _rangeWeaponModel.Reload;
            _rangeWeaponView.Enable();
            _rangeWeaponModel.ModelUpdated += UpdateViewDescription;
            UpdateViewDescription();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _rangeWeaponModel.ShootSingle;
            _inventoryInput.FirstActionButtonHold -= _rangeWeaponModel.ShootAutomatic;
            _inventoryInput.FirstActionButtonUp -= _rangeWeaponModel.CancelShoot;
            _inventoryInput.SecondActionButtonDown -= ZoomIn;
            _inventoryInput.SecondActionButtonUp -= ZoomOut;
            _inventoryInput.ReloadButtonDown -= _rangeWeaponModel.Reload;
            ZoomOut();
            _rangeWeaponView.Disable();
            _rangeWeaponModel.ModelUpdated -= UpdateViewDescription;
        }

        private void ZoomIn()
        {
            _rangeWeaponModel.ZoomIn();
            _rangeWeaponView.EnableScope();
        }

        private void ZoomOut()
        {
            _rangeWeaponModel.ZoomOut();
            _rangeWeaponView.DisableScope();
        }

        public void Dispose()
        {
        }

        private void UpdateViewDescription()
        {
            _rangeWeaponView.UpdateAmmoText($"{_rangeWeaponModel.BulletsInMagazine}/{_rangeWeaponModel.TotalBullets}");
        }
    }
}