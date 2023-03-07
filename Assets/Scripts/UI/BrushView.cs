using Core;
using GamePlay;
using UnityEngine;

namespace UI
{
    public class BrushView : IInventoryItemView, ILeftMouseButtonHoldHandler
    {
        public Sprite Icon { get; }
        public GameObject Pointer { get; set; }
        private readonly GameObject _palette;
        private readonly ColoringBrush _coloringBrush;
        private Color32 _currentColor;

        public BrushView(ColoringBrush coloringBrush)
        {
            GlobalEvents.OnPaletteUpdate.AddListener(color => _currentColor = color);
            _coloringBrush = coloringBrush;
            _palette = GameObject.Find("Canvas/GamePlay/Palette");
            Icon = Resources.Load<Sprite>("Sprites/brush");
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

        public void OnLeftMouseButtonHold()
        {
            _coloringBrush.PaintBlock(_currentColor);
        }
    }
}