using Core;
using GamePlay;
using Infrastructure.AssetManagement;
using UnityEngine;

namespace UI
{
    public class BrushView : IInventoryItemView, ILeftMouseButtonHoldHandler
    {
        public Sprite Icon { get; }
        private readonly GameObject _palette;
        private readonly ColoringBrush _coloringBrush;
        private Color32 _currentColor;

        public BrushView(Camera camera, float placeDistance)
        {
            GlobalEvents.OnPaletteUpdate.AddListener(color => _currentColor = color);
            _coloringBrush = new ColoringBrush(camera, placeDistance);
            _palette = GameObject.Find("Canvas/GamePlay/Palette");
            Icon = Resources.Load<Sprite>(SpritePath.BrushPath);
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
            _coloringBrush.PaintBlock(_currentColor);
        }
    }
}