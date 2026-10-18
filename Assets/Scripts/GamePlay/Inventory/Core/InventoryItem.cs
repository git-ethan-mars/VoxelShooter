using System;
using Data;
using Mirror;
using Networking.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;

namespace GamePlay
{
	[SelectionBase]
	public abstract class InventoryItem : NetworkBehaviour
	{
		[field: SerializeField] private MeshRenderer[] renderers;
		private readonly SyncReactiveProperty<bool> _isSelected = new SyncReactiveProperty<bool>();

		[SyncVar] private GameClass _ownerClass;

		[field: SerializeField] public MeshFilter[] MeshFilters { get; private set; }

		// Offset of the model from the first person anchor; the anchor looks along the view direction
		[field: SerializeField] public Vector3 HoldPosition { get; private set; }
		[field: SerializeField] public Vector3 HoldRotation { get; private set; }

		// Offset of the model from the right palm of other players' characters; the grip looks along the aim
		[field: SerializeField] public Vector3 GripPosition { get; private set; }
		[field: SerializeField] public Vector3 GripRotation { get; private set; }

		// Point of the model held by the left hand; zero leaves the left hand to the animation
		[field: SerializeField] public Vector3 SupportHandPosition { get; private set; }
		public abstract ItemType Type { get; }
		public bool IsSelected => _isSelected.Value;

		public PlayerId? OwnerId
		{
			get
			{
				if (netIdentity.connectionToClient is null)
				{
					return null;
				}

				return new PlayerId(netIdentity.connectionToClient.connectionId);
			}
		}

		protected InventoryItemConfigure Configure { get; private set; }
		protected GameClass OwnerClass => _ownerClass;
		protected bool IsLocalItem => isOwned;

		[Inject]
		private void Construct(CharacterProvider characterProvider, IStaticDataService staticData)
		{
			Configure = staticData.GetItemConfigure<InventoryItemConfigure>(Type);
		}

		[Server]
		public void Initialize(GameClass ownerClass)
		{
			_ownerClass = ownerClass;
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
			Array.ForEach(renderers, part => part.enabled = true);
		}

		private void OnDeselected()
		{
			enabled = false;
			Array.ForEach(renderers, part => part.enabled = false);
		}
	}
}
