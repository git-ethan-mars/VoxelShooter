using Data;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Networking.Synchronization;
using PlayerLogic;
using Rendering;
using TMPro;
using UI;
using UnityEngine;

namespace Inventory
{
    public class MeleeWeaponView : IInventoryItemView, ILeftMouseButtonDownHandler, ILeftMouseButtonHoldHandler, IUpdated
    { 
        public Sprite Icon { get; }
        private GameObject Model { get; }
        private readonly MeleeWeaponData _meleeWeapon;
        private readonly Transform _attackPoint;
        private readonly Camera _fpsCam;
        private readonly IGameFactory _gameFactory;
        private readonly MeleeWeaponSynchronization _meleeWeaponSynchronization;
        private readonly IInputService _inputService;
        private MapSynchronization _mapSynchronization;
        private CubeRenderer _cubeRenderer;


        public MeleeWeaponView(IGameFactory gameFactory, IInputService inputService, Camera camera,
            Transform itemPosition, GameObject player, GameObject hud, MeleeWeaponData configuration, CubeRenderer cubeRenderer,
            MapSynchronization mapSynchronization)
        {
            _gameFactory = gameFactory;
            _inputService = inputService;
            _meleeWeapon = configuration;
            _cubeRenderer = cubeRenderer;
            _cubeRenderer.PlaceDistance = _meleeWeapon.Range;
            _mapSynchronization = mapSynchronization;
            Model = _gameFactory.CreateGameModel(_meleeWeapon.Prefab, itemPosition);
            Model.SetActive(false);
            Icon = _meleeWeapon.Icon;
            _attackPoint = Model.GetComponentInChildren<Transform>();
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
            DestroyBlock();
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _meleeWeaponSynchronization.CmdHitSingle(ray, _meleeWeapon.ID);
        }

        public void OnLeftMouseButtonHold()
        {
            var ray = _fpsCam.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            _meleeWeaponSynchronization.CmdHitAutomatic(ray, _meleeWeapon.ID);
        }

        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube(false);
        }
        
        
        private void DestroyBlock()
        {
            var raycastResult = _cubeRenderer.GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            _mapSynchronization.UpdateBlocksOnServer(
                new [] {Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2)},
                new[] {new BlockData(BlockColor.Empty)});
        }
    }
}