using Data;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class BrushView : IInventoryItemView, ILeftMouseButtonHoldHandler
    {
        public Sprite Icon { get; }
        private readonly GameObject _palette;
        private Color32 _currentColor;
        private readonly float _placeDistance;
        private readonly CubeRenderer _cubeRenderer;
        

        public BrushView(CubeRenderer cubeRenderer,
            GameObject palette, BrushItem configuration)
        {
            palette.GetComponent<PaletteCreator>().OnColorUpdate += UpdateColor;
            _cubeRenderer = cubeRenderer;
            _palette = palette;
            Icon = configuration.inventoryIcon;
        }

        public void Select()
        {
            _palette.SetActive(true);
        }

        public void Unselect()
        {
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
                /*_mapSynchronization.UpdateBlocksOnServer(
                    new[] {Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2)},
                    new[] {new BlockData(color)});*/
            }
        }

        private void UpdateColor(Color32 newColor)
        {
            _currentColor = newColor;
        }
    }
}