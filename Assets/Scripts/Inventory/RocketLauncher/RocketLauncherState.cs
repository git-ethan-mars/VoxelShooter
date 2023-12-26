using CameraLogic;
using Data;
using UI;

namespace Inventory.RocketLauncher
{
    public class RocketLauncherState : IInventoryItemState
    {
        private readonly IInventoryInput _inventoryInput;
        public IInventoryItemModel ItemModel => _rocketLauncherModel;
        public IInventoryItemView ItemView => _rocketLauncherView;

        private readonly RocketLauncherModel _rocketLauncherModel;
        private readonly RocketLauncherView _rocketLauncherView;

        public RocketLauncherState(IInventoryInput inventoryInput, RayCaster rayCaster, RocketLauncherItem configure,
            Hud hud, RocketLauncherItemData rocketLauncherItemData)
        {
            _inventoryInput = inventoryInput;
            _rocketLauncherModel = new RocketLauncherModel(rayCaster, rocketLauncherItemData);
            _rocketLauncherView = new RocketLauncherView(configure, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _rocketLauncherModel.Shoot;
            _inventoryInput.ReloadButtonDown += _rocketLauncherModel.Reload;
            _rocketLauncherView.Enable();
            _rocketLauncherModel.ModelUpdated += UpdateViewDescription;
            UpdateViewDescription();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _rocketLauncherModel.Shoot;
            _inventoryInput.ReloadButtonDown -= _rocketLauncherModel.Reload;
            _rocketLauncherView.Disable();
            _rocketLauncherModel.ModelUpdated -= UpdateViewDescription;
        }

        public void Dispose()
        {
        }
        
        private void UpdateViewDescription()
        {
            _rocketLauncherView.UpdateAmmoText($"{_rocketLauncherModel.ChargedRockets}/{_rocketLauncherModel.CarriedRockets}");
        }
    }
}