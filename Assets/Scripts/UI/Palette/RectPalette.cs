using System;
using System.Collections.Generic;
using Data;
using R3;
using UnityEngine;

namespace UI
{
	public class RectPalette
	{
		private const int RecentColorCapacity = 8;

		private readonly Color32[] _colors;
		private readonly List<Color32> _recentColors = new List<Color32>();
		private readonly List<Color32> _mapColors = new List<Color32>();
		private readonly ReactiveProperty<Color32> _selectedColor = new ReactiveProperty<Color32>();
		private readonly Subject<Unit> _recentColorsChanged = new Subject<Unit>();
		private readonly Subject<Unit> _mapColorsChanged = new Subject<Unit>();

		private int _pointerRow;
		private int _pointerColumn;
		private int _recentIndex;

		public int ColumnCount { get; }
		public IReadOnlyList<Color32> Colors => _colors;
		public IReadOnlyList<Color32> RecentColors => _recentColors;
		public IReadOnlyList<Color32> MapColors => _mapColors;
		public ReadOnlyReactiveProperty<Color32> SelectedColor => _selectedColor;
		public Observable<Unit> RecentColorsChanged => _recentColorsChanged;
		public Observable<Unit> MapColorsChanged => _mapColorsChanged;

		private int RowCount => (_colors.Length + ColumnCount - 1) / ColumnCount + (HasMapRow ? 1 : 0);
		private bool HasMapRow => _mapColors.Count > 0;

		public RectPalette(RectPaletteData data)
		{
			if (data.Colors.Count == 0 || data.Column <= 0)
			{
				throw new ArgumentException("Palette must contain colors and a positive column count", nameof(data));
			}

			ColumnCount = data.Column;
			_colors = new Color32[data.Colors.Count];

			for (int i = 0; i < _colors.Length; i++)
			{
				_colors[i] = data.Colors[i];
			}

			for (int i = 0; i < data.StartRecentColors.Count && i < RecentColorCapacity; i++)
			{
				_recentColors.Add(data.StartRecentColors[i]);
			}

			_selectedColor.Value = _recentColors.Count > 0 ? _recentColors[0] : _colors[0];
			FocusSelectedColor();
		}

		public static bool AreSame(Color32 first, Color32 second)
		{
			return first.r == second.r && first.g == second.g && first.b == second.b && first.a == second.a;
		}

		public void MovePointer(int columnOffset, int rowOffset, bool isExpanded)
		{
			if (isExpanded)
			{
				_pointerRow = Mathf.Clamp(_pointerRow + rowOffset, 0, RowCount - 1);
				_pointerColumn = Mathf.Clamp(_pointerColumn + columnOffset, 0, GetRowLength(_pointerRow) - 1);
				_selectedColor.Value = GetColor(_pointerRow, _pointerColumn);
				return;
			}

			if (_recentColors.Count == 0 || columnOffset == 0)
			{
				return;
			}

			_recentIndex = Mathf.Clamp(_recentIndex + columnOffset, 0, _recentColors.Count - 1);
			_selectedColor.Value = _recentColors[_recentIndex];
			FocusSelectedColor();
		}

		public void CommitSelectedColor()
		{
			AddRecentColor(_selectedColor.Value);
		}

		public void PickColor(Color32 color)
		{
			_selectedColor.Value = color;
			AddRecentColor(color);
			FocusSelectedColor();
		}

		public void SetMapColors(IEnumerable<Color32> colors)
		{
			_mapColors.Clear();

			foreach (Color32 color in colors)
			{
				if (_mapColors.Count == ColumnCount)
				{
					break;
				}

				_mapColors.Add(color);
			}

			FocusSelectedColor();
			_mapColorsChanged.OnNext(Unit.Default);
		}

		public void FocusSelectedColor()
		{
			for (int row = 0; row < RowCount; row++)
			{
				for (int column = 0; column < GetRowLength(row); column++)
				{
					if (AreSame(GetColor(row, column), _selectedColor.Value))
					{
						_pointerRow = row;
						_pointerColumn = column;
						return;
					}
				}
			}

			_pointerRow = Mathf.Clamp(_pointerRow, 0, RowCount - 1);
			_pointerColumn = Mathf.Clamp(_pointerColumn, 0, GetRowLength(_pointerRow) - 1);
		}

		private void AddRecentColor(Color32 color)
		{
			int existingIndex = _recentColors.FindIndex(recentColor => AreSame(recentColor, color));

			if (existingIndex == 0)
			{
				_recentIndex = 0;
				return;
			}

			if (existingIndex > 0)
			{
				_recentColors.RemoveAt(existingIndex);
			}
			else if (_recentColors.Count == RecentColorCapacity)
			{
				_recentColors.RemoveAt(_recentColors.Count - 1);
			}

			_recentColors.Insert(0, color);
			_recentIndex = 0;
			_recentColorsChanged.OnNext(Unit.Default);
		}

		private int GetRowLength(int row)
		{
			if (HasMapRow)
			{
				if (row == 0)
				{
					return _mapColors.Count;
				}

				row--;
			}

			return Mathf.Min(ColumnCount, _colors.Length - row * ColumnCount);
		}

		private Color32 GetColor(int row, int column)
		{
			if (HasMapRow)
			{
				if (row == 0)
				{
					return _mapColors[column];
				}

				row--;
			}

			return _colors[row * ColumnCount + column];
		}
	}
}
