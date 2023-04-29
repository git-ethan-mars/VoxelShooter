using Data;
using Infrastructure.Factory;
using PlayerLogic;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class BlockView : IInventoryItemView, ILeftMouseButtonDownHandler, IUpdated
    {
        public Sprite Icon { get; }
        private GameObject Model { get; }
        private Color32 _currentColor;
        private readonly CubeRenderer _cubeRenderer;
        private readonly MapSynchronization _mapSynchronization;
        private int _blockCount;
        private readonly GameObject _blockInfo;


        public BlockView(InventoryModelFactory gameFactory, CubeRenderer cubeRender, MapSynchronization mapSynchronization, GameObject hud, BlockItem configuration, GameObject player)
        {
            _cubeRenderer = cubeRender;
            _mapSynchronization = mapSynchronization;
            var palette = hud.GetComponent<Hud>().palette;
            _blockInfo = hud.GetComponent<Hud>().blockInfo;
            Icon = configuration.inventoryIcon;
            Model = gameFactory.CreateGameModel(configuration.prefab, player.GetComponent<Player>().itemPosition);
            Model.SetActive(false);
            palette.GetComponent<PaletteCreator>().OnColorUpdate += UpdateColor;
            _blockCount = player.GetComponent<Player>().blockCount;
        }

        public void Select()
        {
            Model.SetActive(true);
            _blockInfo.SetActive(true);
            _cubeRenderer.EnableCube();
        }

        public void Unselect()
        {
            Model.SetActive(false);
            _blockInfo.SetActive(false);
            _cubeRenderer.DisableCube();
        }

        public void OnLeftMouseButtonDown()
        {
            PlaceBlock(_currentColor);
        }

        public void InnerUpdate()
        {
            _cubeRenderer.UpdateCube(true);
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

        
    }
}