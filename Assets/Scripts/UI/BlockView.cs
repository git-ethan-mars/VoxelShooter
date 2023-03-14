using Core;
using GamePlay;
using UnityEngine;

namespace UI
{
    public class BlockView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler, IInnerUpdate
    {
        public Sprite Icon { get; }
        private Color32 _currentColor;
        public GameObject Pointer { get; set; }
        private readonly BlockPlacement _blockPlacement;
        private readonly GameObject _palette;

        public BlockView(BlockPlacement blockPlacement)
        {
            GlobalEvents.OnPaletteUpdate.AddListener(color => _currentColor = color);
            _blockPlacement = blockPlacement;
            _palette = GameObject.Find("Canvas/GamePlay/Palette");
            Icon = Resources.Load<Sprite>("Sprites/Blocks/#C0C0C0");
        }

        public void Select()
        {
            _palette.SetActive(true);
            Pointer.SetActive(true);
            _blockPlacement.EnableCube();
        }

        public void Unselect()
        {
            _palette.SetActive(false);
            Pointer.SetActive(false);
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