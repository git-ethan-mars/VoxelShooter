using Data;
using Infrastructure.Factory;
using Networking.Synchronization;
using PlayerLogic;
using Rendering;
using UnityEngine;

namespace Inventory
{
    public class MeleeWeaponView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler,
        IUpdated
    {
        public Sprite Icon { get; }
        private readonly MeleeWeaponData _meleeWeapon;
        private readonly IGameFactory _gameFactory;
        private readonly MeleeWeaponSynchronization _meleeWeaponSynchronization;
        private readonly CubeRenderer _cubeRenderer;
        private readonly Raycaster _raycaster;


        public MeleeWeaponView(Raycaster raycaster, Player player, MeleeWeaponData configuration)
        {
            _meleeWeapon = configuration;
            _raycaster = raycaster;
            _cubeRenderer = new CubeRenderer(player.GetComponent<LineRenderer>(), raycaster, player.placeDistance);
            Icon = _meleeWeapon.Icon;
            _meleeWeaponSynchronization = player.GetComponent<MeleeWeaponSynchronization>();
        }

        public void Select()
        {
            _cubeRenderer.EnableCube();
        }


        public void Unselect()
        {
            _cubeRenderer.DisableCube();
        }

        public void OnLeftMouseButtonDown()
        {
            _meleeWeaponSynchronization.CmdHit(_raycaster.CentredRay, _meleeWeapon.ID, false);
        }

        public void OnRightMouseButtonDown()
        {
            if (_meleeWeapon.HasStrongHit)
            {
                _meleeWeaponSynchronization.CmdHit(_raycaster.CentredRay, _meleeWeapon.ID, true);
            }
        }

        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube(false);
        }
    }
}