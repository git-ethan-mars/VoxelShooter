using CameraLogic;
using Data;
using PlayerLogic;

namespace Inventory.MeleeWeapon
{
    public class MeleeWeaponState : IInventoryItemState
    {
        private readonly IInventoryInput _inventoryInput;
        private readonly RayCaster _rayCaster;
        private readonly Player _player;
        private readonly MeleeWeaponItemData _configuration;
        public IInventoryItemModel ItemModel => _meleeWeaponModel;
        public IInventoryItemView ItemView => _meleeWeaponView;
        private readonly MeleeWeaponModel _meleeWeaponModel;
        private readonly MeleeWeaponView _meleeWeaponView;

        public MeleeWeaponState(IInventoryInput inventoryInput, RayCaster rayCaster, Player player,
            MeleeWeaponItem configuration)
        {
            _inventoryInput = inventoryInput;
            _meleeWeaponModel = new MeleeWeaponModel(rayCaster);
            _meleeWeaponView = new MeleeWeaponView(rayCaster, player, configuration);
        }

        public void Enter()
        {
            _inventoryInput.FirstActionButtonDown += _meleeWeaponModel.WeakHit;
            _inventoryInput.SecondActionButtonDown += _meleeWeaponModel.StrongHit;
            _meleeWeaponView.Enable();
        }

        public void Update()
        {
            _meleeWeaponView.Update();
        }

        public void Exit()
        {
            _inventoryInput.FirstActionButtonDown -= _meleeWeaponModel.WeakHit;
            _inventoryInput.SecondActionButtonDown -= _meleeWeaponModel.StrongHit;
            _meleeWeaponView.Disable();
        }

        public void Dispose()
        {
        }
    }
}