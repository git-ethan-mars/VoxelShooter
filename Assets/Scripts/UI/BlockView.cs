using Core;
using GamePlay;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace UI
{
    public class BlockView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler, IUpdated
    {
        public Sprite Icon { get; }
        private Color32 _currentColor;
        private readonly BlockPlacement _blockPlacement;
        private readonly GameObject _palette;

        public BlockView(LineRenderer lineRenderer, Camera camera, float placeDistance)
        {
            GlobalEvents.OnPaletteUpdate.AddListener(color => _currentColor = color);
            _blockPlacement = new BlockPlacement(lineRenderer, camera, placeDistance);
            _palette = GameObject.Find("Canvas/GamePlay/Palette");
            Icon = Resources.Load<Sprite>(SpritePath.BlockPath);
        }

        public void Select()
        {
            _palette.SetActive(true);
            _blockPlacement.EnableCube();
        }

        public void Unselect()
        {
            _palette.SetActive(false);
            _blockPlacement.DisableCube();
        }

        public void OnLeftMouseButtonDown()
        {
            _blockPlacement.PlaceBlock(_currentColor);
        }

        public void OnRightMouseButtonDown()
        {
            _blockPlacement.DestroyBlock();
        }

        public void InnerUpdate()
        {
            _blockPlacement.UpdateCube();
        }
    }
}