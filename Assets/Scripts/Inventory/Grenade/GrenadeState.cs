using CameraLogic;
using Data;
using UI;

namespace Inventory.Grenade
{
    public class GrenadeState : IInventoryItemState
    {
        public IInventoryItemModel ItemModel => _grenadeModel;
        private readonly GrenadeModel _grenadeModel;
        public IInventoryItemView ItemView => _grenadeView;
        private readonly GrenadeView _grenadeView;
        private readonly IInventoryInput _inventoryInput;


        public GrenadeState(IInventoryInput inventoryInput, RayCaster rayCaster, GrenadeItem configure,
            GrenadeItemData data, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _grenadeModel = new GrenadeModel(rayCaster, configure, data);
            _grenadeView = new GrenadeView(configure, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _grenadeModel.PullPin;
            _inventoryInput.FirstActionButtonUp += _grenadeModel.Throw;
            _grenadeView.Enable();
            _grenadeModel.ModelChanged += UpdateViewDescription;
            UpdateViewDescription();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _grenadeModel.PullPin;
            _inventoryInput.FirstActionButtonUp -= _grenadeModel.Throw;
            _grenadeView.Disable();
            _grenadeModel.ModelChanged -= UpdateViewDescription;
        }

        public void Dispose()
        {
        }

        private void UpdateViewDescription()
        {
            _grenadeView.UpdateAmmoText($"{_grenadeModel.Amount}");
        }
    }
}