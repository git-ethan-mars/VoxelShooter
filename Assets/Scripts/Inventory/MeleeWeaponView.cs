using Data;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Requests;
using PlayerLogic;
using Rendering;
using UnityEngine;

namespace Inventory
{
    public class MeleeWeaponView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler,
        IUpdated
    {
        public Sprite Icon { get; }
        private readonly IGameFactory _gameFactory;
        private readonly CubeRenderer _cubeRenderer;
        private readonly Raycaster _raycaster;

        public MeleeWeaponView(Raycaster raycaster, Player player, MeleeWeaponData configuration)
        {
            _raycaster = raycaster;
            _cubeRenderer = new CubeRenderer(player.GetComponent<LineRenderer>(), raycaster, player);
            Icon = configuration.Icon;
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
            NetworkClient.Send(new HitRequest(_raycaster.CentredRay, false));
        }

        public void OnRightMouseButtonDown()
        {
            NetworkClient.Send(new HitRequest(_raycaster.CentredRay, true));
        }

        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube(false);
        }
    }
}