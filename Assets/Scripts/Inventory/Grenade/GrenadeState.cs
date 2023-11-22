using Data;
using UI;

namespace Inventory
{
    public class GrenadeState : IInventoryItemState
    {
        public IInventoryItemModel ItemModel => _grenadeModel;
        private readonly GrenadeModel _grenadeModel;
        public IInventoryItemView ItemView => _grenadeView;
        private readonly GrenadeView _grenadeView;
        private readonly IInventoryInput _inventoryInput;


        public GrenadeState(IInventoryInput inventoryInput, RayCaster rayCaster, GrenadeItem configure, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _grenadeModel = new GrenadeModel(rayCaster, configure);
            _grenadeView = new GrenadeView(configure, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _grenadeModel.PullPin;
            _inventoryInput.FirstActionButtonUp += _grenadeModel.Throw;
            _grenadeModel.Count.ValueChanged += _grenadeView.OnCountChanged;
            _grenadeView.Enable();
        }

        public void Update()
        {
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _grenadeModel.PullPin;
            _inventoryInput.FirstActionButtonUp -= _grenadeModel.Throw;
            _grenadeView.Disable();
        }
    }
}