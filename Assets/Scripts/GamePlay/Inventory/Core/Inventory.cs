using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using R3;
namespace GamePlay.Core
{
	public class Inventory : NetworkBehaviour
	{
		private readonly SyncList<InventoryItem> _items = new SyncList<InventoryItem>();
		[SyncVar(hook = nameof(OnSlotSelected))] private int _activeSlotIndex;
		private readonly Subject<(int oldItemIndex, int newItemIndex)> _onSlotSelected = new Subject<(int, int)>();

		public IReadOnlyList<InventoryItem> Items => _items;
		public Observable<int> ItemAdded { get; private set; }
		public Observable<(int oldSlotIndex, int slotIndex)> SlotSelected => _onSlotSelected;

		[Server]
		public void Initialize(IEnumerable<InventoryItem> items)
		{
			_items.AddRange(items);
			_items[0].Select();
		}

		public override void OnStartClient()
		{
			base.OnStartClient();

			if (isServer)
			{
				foreach (InventoryItem item in _items)
				{
					item.netIdentity.AssignClientAuthority(connectionToClient);
				}
			}

			for (var i = 0; i < _items.Count; i++)
			{
				OnItemAdded(i);
			}

			ItemAdded = Observable.FromEvent<int>(handler => _items.OnAdd += handler, handler => _items.OnAdd -= handler);
			ItemAdded.Subscribe(OnItemAdded).AddTo(this);
		}

		[Command]
		public void CmdSelectSlot(int slotIndex)
		{
			SelectSlot(slotIndex);
		}

		[Command]
		public void CmdChangeToNextInventorySlot()
		{
			int inventorySize = Items.Count;
			int currentSlot = (_activeSlotIndex + 1 + inventorySize) % inventorySize;
			SelectSlot(currentSlot);
		}

		[Command]
		public void CmdChangeToPreviousInventorySlot()
		{
			int inventorySize = Items.Count;
			int currentSlot = (_activeSlotIndex - 1 + inventorySize) % inventorySize;
			SelectSlot(currentSlot);
		}

		public T GetItem<T>() where T : InventoryItem
		{
			return _items.OfType<T>().FirstOrDefault();
		}

		[Server]
		private void SelectSlot(int slotIndex)
		{
			if (slotIndex < 0 || slotIndex >= _items.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(slotIndex), $"Slot number should be between 0 and {_items.Count}");
			}

			if (_activeSlotIndex == slotIndex)
			{
				return;
			}

			_items[_activeSlotIndex].Deselect();
			_activeSlotIndex = slotIndex;
			_items[_activeSlotIndex].Select();
		}

		private void OnItemAdded(int index)
		{
			InventoryItem item = _items[index];
			item.transform.SetParent(transform, false);
		}

		private void OnSlotSelected(int oldSlotIndex, int slotIndex)
		{
			_onSlotSelected.OnNext((oldSlotIndex, slotIndex));
		}
	}
}