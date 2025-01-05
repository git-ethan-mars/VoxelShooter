using UI.Inventory;
using UnityEngine;

namespace UI
{
	public class PaletteView : ListView<PaletteElementView>
	{
		public void Show()
		{	
			gameObject.SetActive(true);
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}

		public void SelectElement(int index)
		{
			Debug.Log($"Selected Element {index}");
		}
	}
}