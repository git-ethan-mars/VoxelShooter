using System.Collections.Generic;
using Data;
using Networking.Synchronization;
using Rendering;
using UI;
using UnityEngine;

namespace Inventory
{
    public class BlockView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler, IUpdated
    {
        public Sprite Icon { get; }
        public GameObject Model { get; }
        private Color32 _currentColor;
        private readonly GameObject _palette;
        private readonly CubeRenderer _cubeRenderer;
        private readonly MapSynchronization _mapSynchronization;
        private readonly BlockItem _blockItem;


        public BlockView(CubeRenderer cubeRender, MapSynchronization mapSynchronization, GameObject palette, BlockItem configuration)
        {
            _cubeRenderer = cubeRender;
            _mapSynchronization = mapSynchronization;
            _palette = palette;
            Icon = configuration.icon;
            palette.GetComponent<PaletteCreator>().OnColorUpdate += UpdateColor;
        }

        public void Select()
        {
            _palette.SetActive(true);
            _cubeRenderer.EnableCube();
        }

        public void Unselect()
        {
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
            _mapSynchronization.ChangeBlockState(
                new List<Vector3Int>() {Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2)},
                new[] {new BlockData() {Color = color}});
        }


        private void UpdateColor(Color32 newColor)
        {
            _currentColor = newColor;
        }

        private void DestroyBlock()
        {
            var raycastResult = _cubeRenderer.GetRayCastHit(out var raycastHit);
            if (!raycastResult) return;
            _mapSynchronization.ChangeBlockState(
                new List<Vector3Int>() {Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2)},
                new[] {new BlockData() {Color = BlockColor.Empty}});
        }
    }
}