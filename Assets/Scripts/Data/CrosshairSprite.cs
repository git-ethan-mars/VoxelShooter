using UnityEngine;
namespace Data
{
	public class CrosshairSprite
	{
		public readonly int ID;
		public readonly Sprite Sprite;

		public CrosshairSprite(int id, Sprite sprite)
		{
			ID = id;
			Sprite = sprite;
		}
	}
}