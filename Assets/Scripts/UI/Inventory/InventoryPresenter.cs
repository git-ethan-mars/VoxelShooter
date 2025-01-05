using System.Collections.Generic;
using GamePlay;
using GamePlay.Services;
using UnityEngine;

namespace UI.Inventory
{
	public sealed class InventoryPresenter : MonoBehaviour
	{
		[SerializeField] 
		private Hud hud;

		[SerializeField] 
		private PaletteView paletteView;

		[SerializeField]
		private InventoryView inventoryView;


		private readonly List<SlotPresenter> _presenters = new();
		private IInputService _inputService;
		private SlotPresenterFactory _presenterFactory;
		private InventorySystem _inventorySystem;

		public void Construct(IInputService inputService)
		{
			_inputService = inputService;
	   		_presenterFactory = new SlotPresenterFactory(hud, paletteView);
		}

		public void Initialize(InventorySystem inventory)
		{
			_inventorySystem = inventory;
			for (var i = 0; i < inventory.Items.Count; i++)
			{
				inventory.Items[i].enabled = false;
				inventory.Items[i].HideModel();
				var slotView = inventoryView.SpawnElement();
				var itemPresenter = _presenterFactory.CreatePresenter(inventory.Items[i], slotView);
				itemPresenter.Initialize();
				_presenters.Add(itemPresenter);
			}
			
			_inventorySystem.ChangeSlot(0);
		}

		public void ResetInventory()
		{
			_inventorySystem.Reset();
			
			for (var i = 0; i < _presenters.Count; i++)
			{
				_presenters[i].Dispose();
			}
			
			_presenters.Clear();
			inventoryView.Clear();
		}

		private void Update()
		{
			var scrollSpeed = _inputService.GetScrollSpeed();
			if (scrollSpeed < 0)
			{
				ChangeToPreviousInventorySlot();
			}

			if (scrollSpeed > 0)
			{
				ChangeToNextInventorySlot();
			}
		}

		private void ChangeToNextInventorySlot()
		{
			var inventorySize = _inventorySystem.Items.Count;
			var currentSlot = (_inventorySystem.ActiveSlotIndex!.Value + 1 + inventorySize) % inventorySize;
			_inventorySystem.ChangeSlot(currentSlot);
		}

		private void ChangeToPreviousInventorySlot()
		{
			var inventorySize = _inventorySystem.Items.Count;
			var currentSlot = (_inventorySystem.ActiveSlotIndex!.Value - 1 + inventorySize) % inventorySize;
			_inventorySystem.ChangeSlot(currentSlot);
		}
	}
}