using System;
using Data;
using Mirror;
using Networking.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace GamePlay.Core
{
	public abstract class InventoryItem : NetworkBehaviour
	{
		[SerializeField] protected MeshRenderer[] model;
		
		public abstract ItemType Type { get; }
		protected InventoryItemConfigure Configure { get; private set; }
		public Observable<bool> IsSelected => _isSelected;
		protected bool IsLocalItem => isOwned;
		private readonly SyncReactiveProperty<bool> _isSelected = new SyncReactiveProperty<bool>();

		[Inject]
		private void Construct(CharacterProvider characterProvider, IStaticDataService staticData)
		{
			Configure = staticData.GetItemConfigure<InventoryItemConfigure>(Type);
		}

		public override void OnStartClient()
		{
			((ReactiveProperty<bool>)_isSelected).Where(isSelected => isSelected)
				.Subscribe(_ => OnSelected())
				.AddTo(this);
			((ReactiveProperty<bool>)_isSelected).Where(isSelected => !isSelected)
				.Subscribe(_ => OnDeselected())
				.AddTo(this);
		}
		
		[Server]
		public virtual void Select()
		{
			_isSelected.Value = true;
		}

		[Server]
		public virtual void Deselect()
		{
			_isSelected.Value = false;
		}

		private void OnSelected()
		{
			enabled = true;
			Array.ForEach(model, part => part.enabled = true);
		}

		private void OnDeselected()
		{
			enabled = false;
			Array.ForEach(model, part => part.enabled = false);
		}
	}
}