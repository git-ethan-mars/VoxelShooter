using Data;
using Infrastructure.Factory;
using Networking.Synchronization;
using Rendering;
using UnityEngine;

namespace Inventory
{
    public class MeleeWeaponView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler,
        IUpdated
    {
        public Sprite Icon { get; }
        private GameObject Model { get; }
        private readonly MeleeWeaponData _meleeWeapon;
        private readonly Camera _fpsCam;
        private readonly IGameFactory _gameFactory;
        private readonly MeleeWeaponSynchronization _meleeWeaponSynchronization;
        private readonly CubeRenderer _cubeRenderer;


        public MeleeWeaponView(InventoryModelFactory modelFactory, Camera camera,
            Transform itemPosition, GameObject player, MeleeWeaponData configuration, LineRenderer lineRenderer)
        {
            _meleeWeapon = configuration;
            _cubeRenderer = new CubeRenderer(lineRenderer, new Raycaster(camera, _meleeWeapon.Range));
            Model = modelFactory.CreateGameModel(_meleeWeapon.Prefab, itemPosition);
            Model.SetActive(false);
            Icon = _meleeWeapon.Icon;
            _fpsCam = camera;
            _meleeWeaponSynchronization = player.GetComponent<MeleeWeaponSynchronization>();
        }

        public void Select()
        {
            Model.SetActive(true);
            _cubeRenderer.EnableCube();
        }


        public void Unselect()
        {
            Model.SetActive(false);
            _cubeRenderer.DisableCube();
        }

        public void OnLeftMouseButtonDown()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _meleeWeaponSynchronization.CmdHit(ray, _meleeWeapon.ID, false);
        }

        public void OnRightMouseButtonDown()
        {
            if (_meleeWeapon.HasStrongHit)
            {
                var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
                _meleeWeaponSynchronization.CmdHit(ray, _meleeWeapon.ID, true);
            }
        }

        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube(false);
        }
    }
}