using UnityEngine;
namespace UI
{
	public class MapView
	{
		public readonly string MapName;
		public readonly Sprite Icon;

		public MapView(string mapName, Sprite icon)
		{
			MapName = mapName;
			Icon = icon;
		}
	}
}