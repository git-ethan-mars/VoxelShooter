using System;
using GamePlay;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class PalettePresenter : MonoBehaviour
	{
		[SerializeField] private GridLayoutGroup grid;
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private PaletteView paletteView;

		private CharacterProvider _characterProvider;
		private IInputService _inputService;
		private RectPalette _palette;

		[Inject]
		private void Construct(IInputService inputService, IStaticDataService staticData, CharacterProvider characterProvider)
		{
			_inputService = inputService;
			_characterProvider = characterProvider;
			_palette = new RectPalette(staticData);
		}

		public void Initialize()
		{
			grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			grid.constraintCount = _palette.ColumnCount;
			for (var i = 0; i < _palette.RowCount; i++)
			{
				for (var j = 0; j < _palette.ColumnCount; j++)
				{
					PaletteElementView element = paletteView.SpawnElement();
					element.Construct(_palette[i, j]);
				}
			}

			DisposableBuilder d = Disposable.CreateBuilder();
			_palette.SelectedCell.Subscribe(t => OnElementSelectedAsync(t.row, t.column)).AddTo(ref d);
			_palette.SelectedColor.Subscribe(OnColorChanged).AddTo(ref d);
			_characterProvider.Character
				.Where(character => character != null)
				.Subscribe(character => character.Inventory.ApplyEffectToItems<Block>(block => block.SelectedColor = _palette.SelectedColor
					.CurrentValue)).AddTo(ref d);
			Observable.EveryUpdate().Where(_ => _inputService.IsUpArrowButtonDown()).Subscribe(_ => _palette.MovePointerUp()).AddTo(ref d);
			Observable.EveryUpdate().Where(_ => _inputService.IsDownArrowButtonDown()).Subscribe(_ => _palette.MovePointerDown()).AddTo(ref d);
			Observable.EveryUpdate().Where(_ => _inputService.IsRightArrowButtonDown()).Subscribe(_ => _palette.MovePointerRight()).AddTo(ref d);
			Observable.EveryUpdate().Where(_ => _inputService.IsLeftArrowButtonDown()).Subscribe(_ => _palette.MovePointerLeft()).AddTo(ref d);
			d.RegisterTo(destroyCancellationToken);
		}

		private async void OnElementSelectedAsync(int row, int column)
		{
			try
			{
				int index = column * _palette.RowCount + row;
				await paletteView.SelectElementAsync(index);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		private void OnColorChanged(Color32 color)
		{
			Character character = _characterProvider.Character.Value;
			
			if (character != null)
			{
				character.Inventory.ApplyEffectToItems<Block>(block => block.SelectedColor = color);
			}
		}
	}
}