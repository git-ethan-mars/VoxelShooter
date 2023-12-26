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

        public TntState(IMeshFactory meshFactory, IInventoryInput inventoryInput, RayCaster rayCaster,
            TntItem configure, TntItemData data, Player player, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _tntModel = new TntModel(rayCaster, player, data);
            _tntView = new TntView(meshFactory, rayCaster, configure, player, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _tntModel.PlaceTnt;
            _tntView.Enable();
            _tntModel.ModelChanged += UpdateViewDescription;
            UpdateViewDescription();
        }

        public void Update()
        {
            _tntView.Update();
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _tntModel.PlaceTnt;
            _tntView.Disable();
            _tntModel.ModelChanged -= UpdateViewDescription;
        }

        public void Dispose()
        {
            _tntView.Dispose();
        }

        private void UpdateViewDescription()
        {
            _tntView.UpdateAmmoText($"{_tntModel.Amount}");
        }
    }
}