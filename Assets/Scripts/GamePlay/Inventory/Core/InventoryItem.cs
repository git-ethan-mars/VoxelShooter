using System;
using Data;
using Mirror;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace GamePlay.Core
{
	public abstract class InventoryItem : NetworkBehaviour
	{
		[SerializeField] private MeshRenderer[] model;
		
		public abstract ItemType Type { get; }
		protected InventoryItemConfigure Configure { get; private set; }
		public Observable<bool> OnSelectStateChanged => _isSelected;
		protected bool IsLocalItem => isOwned;
		private readonly Subject<bool> _isSelected = new Subject<bool>();

		[Inject]
		private void Construct(CharacterProvider characterProvider, IStaticDataService staticData)
		{
			Configure = staticData.GetItemConfigure<InventoryItemConfigure>(Type);
		}
		
		internal virtual void Select()
		{
			enabled = true;
			ShowModel();
			_isSelected.OnNext(true);
		}

		internal virtual void Deselect()
		{
			enabled = false;
			HideModel();
			_isSelected.OnNext(false);
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