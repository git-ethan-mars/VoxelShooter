using CameraLogic;
using Data;
using PlayerLogic;
using Rendering;
using UnityEngine;
using Player = PlayerLogic.Player;

namespace Inventory.MeleeWeapon
{
    public class MeleeWeaponView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly CubeRenderer _cubeRenderer;

        public MeleeWeaponView(RayCaster rayCaster, Player player, MeleeWeaponItem configure)
        {
            _cubeRenderer = new CubeRenderer(player.GetComponent<LineRenderer>(), rayCaster, configure.range);
            Icon = configure.inventoryIcon;
        }

        public void Enable()
        {
            _cubeRenderer.EnableCube();
        }

        public void Update()
        {
            _cubeRenderer.UpdateCube();
        }

        public void Disable()
        {
            _cubeRenderer.DisableCube();
        }
    }
}