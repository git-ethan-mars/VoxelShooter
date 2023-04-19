using Data;
using Infrastructure.Factory;
using Networking.Synchronization;
using PlayerLogic;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class BlockView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler, IUpdated
    {
        public Sprite Icon { get; }
        private GameObject Model { get; }
        private Color32 _currentColor;
        private readonly GameObject _palette;
        private readonly CubeRenderer _cubeRenderer;
        private readonly MapSynchronization _mapSynchronization;
        private int _blockCount;


        public BlockView(IGameFactory gameFactory, CubeRenderer cubeRender, MapSynchronization mapSynchronization, GameObject hud, BlockItem configuration, GameObject player)
        {
            _cubeRenderer = cubeRender;
            _mapSynchronization = mapSynchronization;
            _palette = hud.GetComponent<Hud>().palette;
            Icon = configuration.inventoryIcon;
            Model = gameFactory.CreateGameModel(configuration.prefab, player.GetComponent<Player>().itemPosition);
            Model.SetActive(false);
            _palette.GetComponent<PaletteCreator>().OnColorUpdate += UpdateColor;
            _blockCount = player.GetComponent<Player>().BlockCount;
        }

        public void Select()
        {
            Model.SetActive(true);
            _palette.SetActive(true);
            _cubeRenderer.EnableCube();
        }

        public void Unselect()
        {
            Model.SetActive(false);
            _palette.SetActive(false);
            _cubeRenderer.DisableCube();
        }

        public void OnLeftMouseButtonDown()
        {
            PlaceBlock(_currentColor);
        }

        public void OnRightMouseButtonDown()
        {
            DestroyBlock();
        }

        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube();
        }

        private void PlaceBlock(Color32 color)
        {
            var raycastResult = _cubeRenderer.GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            _mapSynchronization.UpdateBlocksOnServer(
                new[] {Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2)},
                new[] {new BlockData(color)});
    }


        private void UpdateColor(Color32 newColor)
        {
            _currentColor = newColor;
        }

        private void UpdateBlockCount(int blockCount)
        {
            _blockCount = blockCount;
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