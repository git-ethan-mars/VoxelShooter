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
            Hud hud)
        {
            _inventoryInput = inventoryInput;
            _rocketLauncherModel = new RocketLauncherModel(rayCaster, configure);
            _rocketLauncherView = new RocketLauncherView(configure, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _rocketLauncherModel.Shoot;
            _rocketLauncherModel.Count.ValueChanged += _rocketLauncherView.OnCountChanged;
            _rocketLauncherView.Enable();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _rocketLauncherModel.Shoot;
            _rocketLauncherModel.Count.ValueChanged -= _rocketLauncherView.OnCountChanged;
            _rocketLauncherView.Disable();
        }
    }
}