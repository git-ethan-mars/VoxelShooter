using System.Threading;
using GamePlay;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class PaletteView : ListView<PaletteElementView>
	{
		[SerializeField] private GridLayoutGroup grid;

		private CancellationTokenSource _cts;
		private PaletteElementView _selectedElement;
		private IInputService _inputService;
		private RectPalette _rectPalette;
		private CharacterProvider _characterProvider;

		[Inject]
		private void Construct(IInputService inputService, IStaticDataService staticData, CharacterProvider characterProvider)
		{
			_inputService = inputService;
			_rectPalette = new RectPalette(staticData);
			_characterProvider = characterProvider;
		}

		private void Update()
		{
			if (_inputService.IsUpArrowButtonDown())
			{
				_rectPalette.MovePointerUp();
			}

			if (_inputService.IsDownArrowButtonDown())
			{
				_rectPalette.MovePointerDown();
			}

			if (_inputService.IsRightArrowButtonDown())
			{
				_rectPalette.MovePointerRight();
			}

			if (_inputService.IsLeftArrowButtonDown())
			{
				_rectPalette.MovePointerLeft();
			}
		}

		private void OnDestroy()
		{
			_cts?.Dispose();
		}

		public void Initialize()
		{
			Character character = _characterProvider.Character.Value;

			grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			grid.constraintCount = _rectPalette.ColumnCount;
			for (int i = 0; i < _rectPalette.RowCount; i++)
			{
				for (int j = 0; j < _rectPalette.ColumnCount; j++)
				{
					PaletteElementView element = SpawnElement();
					element.Construct(_rectPalette[i, j]);
				}
			}

			_rectPalette.SelectedCell.Subscribe(t => OnElementSelectedAsync(t.row, t.column))
				.AddTo(character);
		}

		private async void OnElementSelectedAsync(int row, int column)
		{
			int index = column * _rectPalette.RowCount + row;

			if (_selectedElement != null)
			{
				_cts.Cancel();
				_cts.Dispose();
			}

			_selectedElement = Items[index];
			Character character = _characterProvider.Character.Value;

			if (character != null)
			{
				character.Inventory.DesiredVoxelColor.Value = _rectPalette.SelectedColor;
			}

			_cts = new CancellationTokenSource();
			await _selectedElement.RunAnimationAsync(_cts.Token);
		}
	}
}
