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
        private readonly BlockModel _blockModel;
        private readonly BlockView _blockView;

        public BlockState(IMeshFactory meshFactory, IInventoryInput inventoryInput, BlockItem configure,
            RayCaster rayCaster, Player player, Hud hud)
        {
            _inventoryInput = inventoryInput;
            _blockModel = new BlockModel(configure, rayCaster, player);
            _blockView = new BlockView(meshFactory, configure, rayCaster, player, hud);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _blockModel.PlaceBlock;
            _blockModel.Count.ValueChanged += _blockView.OnCountChanged;
            _blockModel.BlockColor.ValueChanged += _blockView.ChangeTransparentBlockColor;
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
            _blockModel.BlockColor.ValueChanged += _blockView.ChangeTransparentBlockColor;
            _blockView.Disable();
        }
    }
}