using System.Collections.Generic;
using GamePlay;
using GamePlay.Core;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public sealed class InventoryPresenter : MonoBehaviour
	{
		[SerializeField] private Hud hud;
		[SerializeField] private PaletteView paletteView;
		[SerializeField] private InventoryView inventoryView;

		private readonly List<SlotPresenter> _presenters = new List<SlotPresenter>();
		private IInputService _inputService;
		private CharacterProvider _characterProvider;
		private ISlotPresenterFactory _presenterFactory;

		private GamePlay.Core.Inventory _inventory;

		[Inject]
		private void Construct(IInputService inputService, CameraService cameraService, CharacterProvider characterProvider,
			IStaticDataService staticData, ISlotPresenterFactory slotPresenterFactory)
		{
			_inputService = inputService;
			_characterProvider = characterProvider;
			_presenterFactory = slotPresenterFactory;
		}

		public void Initialize()
		{
			_characterProvider.Character
				.Subscribe(OnCharacterChanged)
				.AddTo(this);
		}

		private void Update()
		{
			float scrollSpeed = _inputService.GetScrollSpeed();

			if (scrollSpeed < 0)
			{
				ChangeToPreviousInventorySlot();
			}

			if (scrollSpeed > 0)
			{
				ChangeToNextInventorySlot();
			}

			if (_inputService.IsFirstSlotButtonPressed())
			{
				_inventory.CmdSelectSlot(0);
			}

			if (_inputService.IsSecondSlotButtonPressed())
			{
				_inventory.CmdSelectSlot(1);
			}

			if (_inputService.IsThirdSlotButtonPressed())
			{
				_inventory.CmdSelectSlot(2);
			}

			if (_inputService.IsFourthSlotButtonPressed())
			{
				_inventory.CmdSelectSlot(3);
			}

			if (_inputService.IsFifthSlotButtonPressed())
			{
				_inventory.CmdSelectSlot(4);
			}
		}

		private void OnCharacterChanged(Character character)
		{
			if (character != null)
			{
				_inventory = character.Inventory;

				for (var i = 0; i < _inventory.Items.Count; i++)
				{
					OnItemAdded(i);
				}
				
				_inventory.ItemAdded.Subscribe(OnItemAdded).AddTo(_inventory);
			}
			else
			{
				ResetView();
			}
		}

		private void OnItemAdded(int index)
		{
			InventoryItem item = _inventory.Items[index];
			SlotView slotView = inventoryView.SpawnElement();
			SlotPresenter itemPresenter = _presenterFactory.CreatePresenter(item, slotView);
			itemPresenter.Initialize();
			_presenters.Add(itemPresenter);

			if (!_inventory.ActiveSlotIndex.HasValue)
			{
				_inventory.CmdSelectSlot(0);
			}
		}

		private void ResetView()
		{
			for (var i = 0; i < _presenters.Count; i++)
			{
				_presenters[i].Dispose();
			}

			_presenters.Clear();
			inventoryView.Clear();
		}

		private void ChangeToNextInventorySlot()
		{
			int inventorySize = _inventory.Items.Count;
			int currentSlot = (_inventory.ActiveSlotIndex!.Value + 1 + inventorySize) % inventorySize;
			_inventory.CmdSelectSlot(currentSlot);
		}

		private void ChangeToPreviousInventorySlot()
		{
			int inventorySize = _inventory.Items.Count;
			int currentSlot = (_inventory.ActiveSlotIndex!.Value - 1 + inventorySize) % inventorySize;
			_inventory.CmdSelectSlot(currentSlot);
		}
	}
}