using System.Collections.Generic;
using GamePlay;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.Rendering.Universal;
namespace UI.Inventory
{
	public sealed class InventoryPresenter : MonoBehaviour
	{
		[SerializeField] private PaletteView paletteView;
		[SerializeField] private InventoryView inventoryView;
		[SerializeField] private Camera canvasCamera;

		private readonly List<SlotPresenter> _presenters = new List<SlotPresenter>();
		private IInputService _inputService;
		private CameraProvider _cameraProvider;
		private CharacterProvider _characterProvider;
		private ISlotPresenterFactory _presenterFactory;

		private GamePlay.Inventory _inventory;
		
		[Inject]
		private void Construct(IInputService inputService, CameraProvider cameraProvider, CharacterProvider characterProvider,
			ISlotPresenterFactory slotPresenterFactory)
		{
			_inputService = inputService;
			_cameraProvider = cameraProvider;
			_characterProvider = characterProvider;
			_presenterFactory = slotPresenterFactory;
		}

		private void OnEnable()
		{
			_characterProvider.Character
				.Subscribe(OnCharacterChanged)
				.AddTo(this);
			UniversalAdditionalCameraData additionalCameraData = _cameraProvider.MainCamera.GetUniversalAdditionalCameraData();
			additionalCameraData.cameraStack.Add(canvasCamera);
		}

		private void Update()
		{
			if (_inventory == null)
			{
				return;
			}
			
			float scrollSpeed = _inputService.GetScrollSpeed();
			
			if (scrollSpeed < 0)
			{
				_inventory.CmdChangeToPreviousInventorySlot();
				inventoryView.ForceShowInventory();
			}

			if (scrollSpeed > 0)
			{
				_inventory.CmdChangeToNextInventorySlot();
				inventoryView.ForceShowInventory();
			}

			for (var i = 0; i < _inventory.Items.Count; i++)
			{
				if (_inputService.IsSlotButtonPressed(i))
				{
					_inventory.CmdSelectSlot(i);
					inventoryView.ForceShowInventory();
				}
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
				_inventory.SlotSelected.Subscribe(OnItemSelected).AddTo(_inventory);
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

			if (index == 0)
			{
				_presenters[0].Select();
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

		private void OnItemSelected((int oldSlotIndex, int slotIndex) value)
		{
			_presenters[value.oldSlotIndex].Deselect();
			_presenters[value.slotIndex].Select();
		}
	}
}