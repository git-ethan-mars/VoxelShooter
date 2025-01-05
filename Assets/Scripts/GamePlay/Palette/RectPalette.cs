using System;
using GamePlay.Services;
using UnityEngine;

namespace GamePlay.Palette
{
	public class RectPalette
	{
		public event Action<int, int> SelectedElementChanged;
		public Color ActiveColor => _colors[_pointerX, _pointerY];
		public Color this[int x, int y] => _colors[x, y];
		public int RowCount => _colors.GetLength(0);
		public int ColumnCount => _colors.GetLength(1);
		private readonly Color[,] _colors;
		private int _pointerX;
		private int _pointerY;

		public RectPalette(IStaticDataService staticData)
		{
			var rectPaletteData = staticData.GetRectPaletteData();
			var column = rectPaletteData.Column;
			var gradients = rectPaletteData.Gradients;
			_pointerX = 0;
			_pointerY = 0;
			_colors = new Color[gradients.Count , column];
			for (var i = 0; i < gradients.Count; i++)
			{
				for (var j = 0; j < column; j++)
				{
					var color = gradients[i].CalculateGradient((float)j / gradients.Count);
					_colors[i, j] = color;
				}
			}
		}

		public void MovePointerLeft()
		{
			if (_pointerX <= 0)
			{
				return;
			}

			_pointerX--;
			SelectedElementChanged?.Invoke(_pointerX, _pointerY);
		}

		public void MovePointerRight()
		{
			if (_pointerX >= _colors.GetLength(1))
			{
				return;
			}

			_pointerX += 1;
			SelectedElementChanged?.Invoke(_pointerX, _pointerY);
		}

		public void MovePointerUp()
		{
			if (_pointerY <= 0)
			{
				return;
			}

			_pointerY -= 1;
			SelectedElementChanged?.Invoke(_pointerX, _pointerY);
		}

		public void MovePointerDown()
		{
			if (_pointerY >= _colors.GetLength(0) - 1)
			{
				return;
			}

			_pointerY += 1;
			SelectedElementChanged?.Invoke(_pointerX, _pointerY);
		}
	}
}