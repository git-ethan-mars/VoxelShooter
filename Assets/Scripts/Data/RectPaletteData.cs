using System.Collections.Generic;
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Rect Palette Data", menuName = "Rect palette")]
	public class RectPaletteData : ScriptableObject
	{
		[SerializeField] private List<Gradient> gradients;
		[SerializeField] private int column;
		public IReadOnlyList<Gradient> Gradients => gradients;

		public int Column => column;
	}
}