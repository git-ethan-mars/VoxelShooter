using GamePlay.Palette;
using GamePlay.Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PalettePresenter : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private PaletteView paletteView;

        private IInputService _inputService;
        private RectPalette _palette;

        public void Construct(IStaticDataService staticData, IInputService inputService)
        {
            _inputService = inputService;
            _palette = new RectPalette(staticData);
        }

        public void Initialize()
        {
            var width = (rectTransform.rect.width - (grid.padding.left + grid.padding.right) -
                         (_palette.ColumnCount - 1) * grid.spacing.x) / _palette.ColumnCount;
            var height = (rectTransform.rect.height - (grid.padding.top + grid.padding.bottom) -
                          (_palette.RowCount - 1) * grid.spacing.x) / _palette.RowCount;
            var elementSize = new Vector2(width, height);
            grid.cellSize = elementSize;
            for (var i = 0; i < _palette.RowCount; i++)
            {
                for (var j = 0; j < _palette.ColumnCount; j++)
                {
                    var element = paletteView.SpawnElement();
                    element.Construct(_palette[i, j], elementSize);
                }
            }
            
            _palette.SelectedElementChanged += OnElementSelected;
        }

        private void Update()
        {
            if (_inputService.IsUpArrowButtonDown())
            {
                _palette.MovePointerUp();
            }

            if (_inputService.IsUpArrowButtonDown())
            {
                _palette.MovePointerDown();
            }

            if (_inputService.IsRightArrowButtonDown())
            {
                _palette.MovePointerRight();
            }

            if (_inputService.IsLeftArrowButtonDown())
            {
                _palette.MovePointerLeft();
            }
        }

        private void OnElementSelected(int row, int column)
        {
            var index = row * _palette.ColumnCount + column;
            paletteView.SelectElement(index);
        }

        private void OnDestroy()
        {
            _palette.SelectedElementChanged -= OnElementSelected;
        }
    }
}
