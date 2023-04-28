using Data;
using Infrastructure.Factory;
using Networking.Synchronization;
using PlayerLogic;
using Rendering;
using UnityEngine;

namespace Inventory
{
    public class MeleeWeaponView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler, IUpdated
    { 
        public Sprite Icon { get; }
        private GameObject Model { get; }
        private readonly MeleeWeaponData _meleeWeapon;
        private readonly Camera _fpsCam;
        private readonly IGameFactory _gameFactory;
        private readonly MeleeWeaponSynchronization _meleeWeaponSynchronization;
        private readonly MapSynchronization _mapSynchronization;
        private readonly CubeRenderer _cubeRenderer;


        public MeleeWeaponView(InventoryModelFactory modelFactory, Camera camera,
            Transform itemPosition, GameObject player, MeleeWeaponData configuration, CubeRenderer cubeRenderer,
            MapSynchronization mapSynchronization)
        {
            _meleeWeapon = configuration;
            _cubeRenderer = cubeRenderer;
            _mapSynchronization = mapSynchronization;
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
            var raycastResult = _cubeRenderer.GetRayCastHit(out var raycastHit);
            if (raycastResult)
            {
                if (_meleeWeapon.IsReady)
                {
                    _mapSynchronization.UpdateBlocksOnServer(
                        new[] { Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2) },
                        new[] { new BlockData(BlockColor.Empty) });
                }
            }
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _meleeWeaponSynchronization.CmdHit(ray, _meleeWeapon.ID, raycastResult);
        }
        
        public void OnRightMouseButtonDown()
        {
            var raycastResult = _cubeRenderer.GetRayCastHit(out var raycastHit);
            if (raycastResult)
            {
                if (_meleeWeapon.IsReady)
                {
                    var targetBlock = Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2);
                    _mapSynchronization.UpdateBlocksOnServer(
                        new[]
                        {
                            targetBlock, new Vector3Int(targetBlock.x, targetBlock.y + 1, targetBlock.z),
                            new Vector3Int(targetBlock.x, targetBlock.y - 1, targetBlock.z)
                        },
                        new[] { new BlockData(BlockColor.Empty), new BlockData(BlockColor.Empty), 
                            new BlockData(BlockColor.Empty) });
                }
            }
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _meleeWeaponSynchronization.CmdHit(ray, _meleeWeapon.ID, raycastResult);
        }

        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube(false);
        }
    }
}