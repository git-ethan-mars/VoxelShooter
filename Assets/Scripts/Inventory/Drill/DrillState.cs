using CameraLogic;
using Data;
using UI;

namespace Inventory.Drill
{
    public class DrillState : IInventoryItemState
    {
        private readonly IInventoryInput _inventoryInput;
        public IInventoryItemModel ItemModel => _drillModel;
        public IInventoryItemView ItemView => _drillView;

        private readonly DrillModel _drillModel;
        private readonly DrillView _drillView;

        public DrillState(IInventoryInput inventoryInput, RayCaster rayCaster, DrillItem configure,
            Hud hud, DrillItemData rocketLauncherItemData)
        {
            _inventoryInput = inventoryInput;
            _drillModel = new DrillModel(rayCaster, rocketLauncherItemData);
            _drillView = new DrillView(configure, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _drillModel.Shoot;
            _inventoryInput.ReloadButtonDown += _drillModel.Reload;
            _drillView.Enable();
            _drillModel.ModelUpdated += UpdateViewDescription;
            UpdateViewDescription();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _drillModel.Shoot;
            _inventoryInput.ReloadButtonDown -= _drillModel.Reload;
            _drillView.Disable();
            _drillModel.ModelUpdated -= UpdateViewDescription;
        }

        public void Dispose()
        {
        }
        
        private void UpdateViewDescription()
        {
            _drillView.UpdateAmmoText($"{_drillModel.ChargedDrills}/{_drillModel.CarriedDrills}");
        }
    }
}