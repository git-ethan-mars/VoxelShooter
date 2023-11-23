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
            RayCaster rayCaster, Player player, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _hud = hud;
            var initialColor = _hud.palette.Color.Value;
            _blockModel = new BlockModel(configure, rayCaster, player, initialColor);
            _blockView = new BlockView(meshFactory, configure, rayCaster, player, hud, initialColor);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _blockModel.PlaceBlock;
            _blockModel.Count.ValueChanged += _blockView.OnCountChanged;
            _blockModel.BlockColor.ValueChanged += _blockView.ChangeTransparentBlockColor;
            _hud.palette.Color.ValueChanged += _blockModel.ChangeColor;
            _blockView.Enable();
        }

        public void Update()
        {
            _blockView.DisplayTransparentBlock();
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _blockModel.PlaceBlock;
            _blockModel.Count.ValueChanged -= _blockView.OnCountChanged;
            _blockModel.BlockColor.ValueChanged -= _blockView.ChangeTransparentBlockColor;
            _hud.palette.Color.ValueChanged -= _blockModel.ChangeColor;
            _blockView.Disable();
        }
    }
}