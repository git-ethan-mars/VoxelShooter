using System.Collections.Generic;
using UnityEngine;
namespace Data
{
	[CreateAssetMenu(fileName = "Crosshair Sprites Collection", menuName = "Sprites/CrosshairSpriteCollection")]
	public class CrosshairSpriteCollection : ScriptableObject
	{
		[SerializeField] private Sprite[] sprites;
		public IReadOnlyList<Sprite> Sprites => sprites;
	}
}