using System.Collections.Generic;
using Data;
using Infrastructure.AssetManagement;
using Networking.Synchronization;
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
        private readonly Camera _camera;
        private readonly float _placeDistance;
        private readonly CubeRenderer _cubeRenderer;
        private readonly MapSynchronization _mapSynchronization;

        public BrushView(CubeRenderer cubeRenderer, MapSynchronization mapSynchronization, IAssetProvider assets, GameObject palette)
        {
            palette.GetComponent<PaletteCreator>().OnColorUpdate += UpdateColor;
            _cubeRenderer = cubeRenderer;
            _mapSynchronization = mapSynchronization;
            _palette = palette;
            Icon = assets.Load<Sprite>(SpritePath.BrushPath);
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
                _mapSynchronization.ChangeBlockState(
                    new List<Vector3Int>() {Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2)},
                    new[] {new BlockData {Color = color}});
            }
        }

        private void UpdateColor(Color32 newColor)
        {
            _currentColor = newColor;
        }
    }
}