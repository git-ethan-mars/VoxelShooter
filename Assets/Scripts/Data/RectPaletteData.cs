using System.Collections.Generic;
using UnityEngine;

namespace Data
{
	[CreateAssetMenu(fileName = "Rect Palette Data", menuName = "Rect palette")]
	public class RectPaletteData : ScriptableObject
	{
		[SerializeField] private List<Color32> colors;
		[SerializeField] private int column;
		[SerializeField] private List<Color32> startRecentColors;

		public IReadOnlyList<Color32> Colors => colors;
		public int Column => column;
		public IReadOnlyList<Color32> StartRecentColors => startRecentColors;
	}
}
