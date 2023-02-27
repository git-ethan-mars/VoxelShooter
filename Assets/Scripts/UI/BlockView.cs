using Core;
using GamePlay;
using UnityEngine;

namespace UI
{
    public class BlockView : IInventoryItemView, ILeftMouseButtonDownHandler, IRightMouseButtonDownHandler
    {
        public Sprite Icon { get; }
        private byte _currentColorId;
        public GameObject Pointer { get; set; }
        private readonly BlockPlacement _blockPlacement;
        private readonly GameObject _palette;

        public BlockView(BlockPlacement blockPlacement)
        {
            GlobalEvents.OnPaletteUpdate.AddListener(colorId => _currentColorId = colorId);
            _blockPlacement = blockPlacement;
            _palette = GameObject.Find("Canvas/GamePlay/Palette");
            Icon = Resources.Load<Sprite>("Sprites/Blocks/#C0C0C0");
        }

        public void Select()
        {
            _palette.SetActive(true);
            Pointer.SetActive(true);
        }

        public void Unselect()
        {
            _palette.SetActive(false);
            Pointer.SetActive(false);
        }

        public void OnLeftMouseButtonDown()
        {
            _blockPlacement.DestroyBlock();
        }

        public void OnRightMouseButtonDown()
        {
            _blockPlacement.PlaceBlock(_currentColorId);
        }
    }
}