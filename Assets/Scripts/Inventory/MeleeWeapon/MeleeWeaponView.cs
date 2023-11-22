using Data;
using Infrastructure.Factory;
using PlayerLogic;
using Rendering;
using UnityEngine;

namespace Inventory
{
    public class MeleeWeaponView : IInventoryItemView
    {
        public Sprite Icon { get; }
        private readonly IGameFactory _gameFactory;
        private readonly CubeRenderer _cubeRenderer;

        public MeleeWeaponView(RayCaster rayCaster, Player player, MeleeWeaponData configure)
        {
            _cubeRenderer = new CubeRenderer(player.GetComponent<LineRenderer>(), rayCaster, player);
            Icon = configure.Icon;
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