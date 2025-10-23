using System;
using System.Collections.Generic;
using Mirror;
using R3;
namespace GamePlay.Core
{
	public class Inventory : NetworkBehaviour
	{
		private readonly SyncList<InventoryItem> _items = new SyncList<InventoryItem>();
		private InventoryItem _selectedItem;

		public IReadOnlyList<InventoryItem> Items => _items;

		public int? ActiveSlotIndex { get; private set; }
		public Observable<int> ItemAdded {get; private set;}

		[Server]
		public void Initialize(IEnumerable<InventoryItem> items)
		{
			_items.AddRange(items);
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
		public void CmdSelectSlot(int slot)
		{
			if (slot < 0 || slot >= _items.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(slot), $"Slot number should be between 0 and {_items.Count}");
			}

			if (ActiveSlotIndex == slot)
			{
				return;
			}

			_selectedItem?.Deselect();
			_selectedItem = _items[slot];
			_selectedItem.Select();
			ActiveSlotIndex = slot;
		}

		[Command]
		public void CmdReset()
		{
			ActiveSlotIndex = null;
			_selectedItem?.Deselect();
			_selectedItem = null;
		}

		private void OnItemAdded(int index)
		{
			InventoryItem item = _items[index];
			item.transform.SetParent(transform, false);
		}
	}
}