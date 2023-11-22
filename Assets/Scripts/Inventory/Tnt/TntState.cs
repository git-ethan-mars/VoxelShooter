using CameraLogic;
using Data;
using Infrastructure.Factory;
using PlayerLogic;
using UI;

namespace Inventory.Tnt
{
    public class TntState : IInventoryItemState
    {
        private readonly IInventoryInput _inventoryInput;
        public IInventoryItemModel ItemModel => _tntModel;
        public IInventoryItemView ItemView => _tntView;
        private readonly TntModel _tntModel;
        private readonly TntView _tntView;

        public TntState(IMeshFactory meshFactory, IInventoryInput inventoryInput, RayCaster rayCaster, TntItem configure, Player player, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _tntModel = new TntModel(rayCaster, player, configure);
            _tntView = new TntView(meshFactory, rayCaster, configure, player, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _tntModel.PlaceTnt;
            _tntModel.Count.ValueChanged += _tntView.OnCountChanged;
            _tntView.Enable();
        }

        public void Update()
        {
            _tntView.Update();
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _tntModel.PlaceTnt;
            _tntModel.Count.ValueChanged -= _tntView.OnCountChanged;
            _tntView.Disable();
        }
    }
}