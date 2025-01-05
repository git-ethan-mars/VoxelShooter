using UnityEngine;
using UnityEngine.UI;

namespace UI.Inventory
{
	public sealed class SlotView : MonoBehaviour
	{
		[SerializeField] 
		private Image icon;

		[SerializeField] 
		private Image boarder;

		public void SetSlotIcon(Sprite sprite)
		{
			icon.sprite = sprite;
		}

		public void Select()
		{
			boarder.enabled = true;
		}

		public void Deselect()
		{
			boarder.enabled = false;
		}
	}
}