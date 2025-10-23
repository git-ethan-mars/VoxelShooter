using System.Linq;
using GamePlay;
using R3;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class PalettePresenter : MonoBehaviour
	{
		[SerializeField] private GridLayoutGroup grid;
		[SerializeField] private PaletteView paletteView;

		private CharacterProvider _characterProvider;

		[Inject]
		private void Construct(CharacterProvider characterProvider)
		{
			_characterProvider = characterProvider;
		}

		public void Initialize()
		{
			_characterProvider.Character.Where(character => character != null).Subscribe(OnCharacterCreated)
				.AddTo(this);
		}

		private void OnCharacterCreated(Character character)
		{
			Block block = character.Inventory.Items.OfType<Block>().FirstOrDefault();

			if (block == null)
			{
				return;
			}

			grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			grid.constraintCount = block.RectPalette.ColumnCount;
			for (var i = 0; i < block.RectPalette.RowCount; i++)
			{
				for (var j = 0; j < block.RectPalette.ColumnCount; j++)
				{
					PaletteElementView element = paletteView.SpawnElement();
					element.Construct(block.RectPalette[i, j]);
				}
			}

			block.RectPalette.SelectedCell.Subscribe(t => OnElementSelectedAsync(t.row, t.column, block.RectPalette))
				.AddTo(character);
		}

		private async void OnElementSelectedAsync(int row, int column, RectPalette rectPalette)
		{
			int index = column * rectPalette.RowCount + row;
			await paletteView.SelectElementAsync(index);
		}
	}
}