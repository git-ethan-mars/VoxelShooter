using CameraLogic;
using Data;
using Infrastructure.Factory;
using PlayerLogic;
using UI;

namespace Inventory.Block
{
    public class BlockState : IInventoryItemState
    {
        public IInventoryItemModel ItemModel => _blockModel;
        public IInventoryItemView ItemView => _blockView;

        private readonly IInventoryInput _inventoryInput;
        private readonly Hud _hud;
        private readonly BlockModel _blockModel;
        private readonly BlockView _blockView;

        public BlockState(IMeshFactory meshFactory, IInventoryInput inventoryInput, BlockItem configure,
            BlockItemData data,
            RayCaster rayCaster, Player player, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _hud = hud;
            var initialColor = _hud.Palette.Color.Value;
            _blockModel = new BlockModel(data, rayCaster, player, initialColor);
            _blockView = new BlockView(meshFactory, configure, rayCaster, player, hud, initialColor);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _blockModel.PlaceBlock;
            _inventoryInput.SecondActionButtonDown += _blockModel.StartLine;
            _inventoryInput.SecondActionButtonUp += _blockModel.EndLine;
            _blockModel.BlockColor.ValueChanged += _blockView.ChangeTransparentBlockColor;
            _hud.Palette.Color.ValueChanged += _blockModel.ChangeColor;
            _blockView.Enable();
            _blockModel.AmountChanged += UpdateViewDescription;
            UpdateViewDescription(_blockModel.Amount);
        }

        public void Update()
        {
            _blockView.DisplayTransparentBlock();
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _blockModel.PlaceBlock;
            _inventoryInput.SecondActionButtonDown -= _blockModel.StartLine;
            _inventoryInput.SecondActionButtonUp -= _blockModel.EndLine;
            _blockModel.BlockColor.ValueChanged -= _blockView.ChangeTransparentBlockColor;
            _hud.Palette.Color.ValueChanged -= _blockModel.ChangeColor;
            _blockView.Disable();
            _blockModel.AmountChanged -= UpdateViewDescription;
        }

        public void Dispose()
        {
            _blockView.Dispose();
        }

        private void UpdateViewDescription(int amount)
        {
            _blockView.UpdateAmmoText($"{amount}");
        }
    }
}