using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class PaletteElementListView : ListView<PaletteElementView>
	{
		public void SetColors(IReadOnlyList<Color32> colors)
		{
			Clear();

			foreach (Color32 color in colors)
			{
				SpawnElement().Construct(color);
			}
		}

		public void Highlight(Color32 color)
		{
			foreach (PaletteElementView item in Items)
			{
				item.SetSelected(RectPalette.AreSame(item.Color, color));
			}
		}
	}
}
