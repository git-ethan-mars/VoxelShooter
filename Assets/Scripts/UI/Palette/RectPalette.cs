using System.Collections.Generic;
using Data;
using R3;
using Services;
using UnityEngine;
using Gradient = Data.Gradient;
namespace UI
{
	public class RectPalette
	{
		private readonly Color[,] _colors;
		private readonly ReactiveProperty<(int row, int column)> _selectedCell;
		private readonly ReactiveProperty<Color32> _selectedColor;

		private int _pointerX;
		private int _pointerY;

		public RectPalette(IStaticDataService staticData)
		{
			RectPaletteData rectPaletteData = staticData.GetRectPaletteData();
			int column = rectPaletteData.Column;
			var gradients = rectPaletteData.Gradients;
			_pointerX = 0;
			_pointerY = 0;
			_colors = new Color[gradients.Count, column];
			for (var i = 0; i < gradients.Count; i++)
			{
				for (var j = 0; j < column; j++)
				{
					Color color = gradients[i].CalculateGradient((float)j / gradients.Count);
					_colors[i, j] = color;
				}
			}

			_selectedColor = new ReactiveProperty<Color32>(_colors[_pointerX, _pointerY]);
			_selectedCell = new ReactiveProperty<(int row, int column)>((_pointerX, _pointerY));
		}

		public Observable<(int row, int column)> SelectedCell => _selectedCell;
		public ReadOnlyReactiveProperty<Color32> SelectedColor => _selectedColor;
		public Color this[int x, int y] => _colors[x, y];
		public int RowCount => _colors.GetLength(0);
		public int ColumnCount => _colors.GetLength(1);

		public void MovePointerLeft()
		{
			if (_pointerX <= 0)
			{
				return;
			}

			_pointerX--;

			_selectedCell.Value = (_pointerX, _pointerY);
			_selectedColor.Value = _colors[_pointerY, _pointerX];
		}

		public void MovePointerRight()
		{
			if (_pointerX + 1 >= _colors.GetLength(1))
			{
				return;
			}

			_pointerX++;

			_selectedCell.Value = (_pointerX, _pointerY);
			_selectedColor.Value = _colors[_pointerY, _pointerX];
		}

		public void MovePointerUp()
		{
			if (_pointerY <= 0)
			{
				return;
			}

			_pointerY--;

			_selectedCell.Value = (_pointerX, _pointerY);
			_selectedColor.Value = _colors[_pointerY, _pointerX];
		}

		public void MovePointerDown()
		{
			if (_pointerY + 1 >= _colors.GetLength(0))
			{
				return;
			}

			_pointerY++;

			_selectedCell.Value = (_pointerX, _pointerY);
			_selectedColor.Value = _colors[_pointerY, _pointerX];
		}
	}
}