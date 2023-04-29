using Data;
using Infrastructure.Factory;
using PlayerLogic;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class BrushView : IInventoryItemView, ILeftMouseButtonHoldHandler
    {
        private GameObject Model { get; }
        public Sprite Icon { get; }
        private readonly GameObject _palette;
        private Color32 _currentColor;
        private readonly float _placeDistance;
        private readonly CubeRenderer _cubeRenderer;
        private readonly MapSynchronization _mapSynchronization;


        public BrushView(InventoryModelFactory gameFactory, CubeRenderer cubeRenderer, MapSynchronization mapSynchronization,
            GameObject palette, BrushItem configuration, GameObject player)
        {
            palette.GetComponent<PaletteCreator>().OnColorUpdate += UpdateColor;
            _cubeRenderer = cubeRenderer;
            _mapSynchronization = mapSynchronization;
            _palette = palette;
            Model = gameFactory.CreateGameModel(configuration.prefab, player.GetComponent<Player>().itemPosition);
            Model.SetActive(false);
            Icon = configuration.inventoryIcon;
        }

        public void Select()
        {
            Model.SetActive(true);
            _palette.SetActive(true);
        }

        public void Unselect()
        {
            Model.SetActive(false);
            _palette.SetActive(false);
        }

        public void OnLeftMouseButtonHold()
        {
            PaintBlock(_currentColor);
        }

        private void PaintBlock(Color32 color)
        {
            var raycastResult = _cubeRenderer.GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            {
                _mapSynchronization.UpdateBlocksOnServer(
                    new[] {Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2)},
                    new[] {new BlockData(color)});
            }
        }

        private void UpdateColor(Color32 newColor)
        {
            _currentColor = newColor;
        }
    }
}