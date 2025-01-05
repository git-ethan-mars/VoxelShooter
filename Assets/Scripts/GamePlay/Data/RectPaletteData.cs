using System.Collections.Generic;
using UnityEngine;

namespace GamePlay.Data
{
	[CreateAssetMenu(fileName = "Rect Palette Data", menuName = "Rect palette")]
	public class RectPaletteData : ScriptableObject
	{
		public IReadOnlyList<Gradient> Gradients => gradients;
		
		[SerializeField] 
		private List<Gradient> gradients;

		public int Column => column;
		
		[SerializeField] 
		private int column;
	}
}