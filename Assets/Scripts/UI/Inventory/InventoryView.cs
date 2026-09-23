using System.Linq;
using UnityEngine;

namespace UI.Inventory
{
	public sealed class InventoryView : ListView<SlotView>
	{
		[SerializeField] private float showDuration = 1f;

		private float _showTimer;
		private bool _isVisible;

		private void Start()
		{
			ForceShowInventory();
		}

		private void Update()
		{
			if (!_isVisible)
			{
				return;
			}

			if (_showTimer < showDuration)
			{
				_showTimer += Time.deltaTime;
			}
			else
			{
				HideInventory();
			}
		}

		public void ForceShowInventory()
		{
			foreach (MeshRenderer meshRenderer in Items.SelectMany(slot => slot.MeshRenderers))
			{
				meshRenderer.enabled = true;
			}

			_showTimer = 0;
			_isVisible = true;
		}

		public void HideInventory()
		{
			foreach (MeshRenderer meshRenderer in Items.SelectMany(slot => slot.MeshRenderers))
			{
				meshRenderer.enabled = false;
			}

			_showTimer = 0;
			_isVisible = false;
		}
	}
}
