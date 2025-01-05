using System;
using UnityEngine;

namespace GamePlay
{
	public abstract class InventoryItem : MonoBehaviour
	{
		[SerializeField] 
		private MeshRenderer[] model;

		public event Action Selected;
		public event Action Deselected;
		public abstract Sprite InventoryIcon { get; }

		internal virtual void Select()
		{
			enabled = true;
			ShowModel();
			Selected?.Invoke();
		}

		internal virtual void Deselect()
		{
			enabled = false;
			HideModel();
			Deselected?.Invoke();
		}

		private void ShowModel()
		{
			Array.ForEach(model, part => part.enabled = true);
		}

		public void HideModel()
		{
			Array.ForEach(model, part => part.enabled = false);
		}
	}
}