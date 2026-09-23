using System;
using GamePlay;
using R3;
using UI.Inventory;
using UnityEngine;
namespace UI.InGameUIStates
{
	public class DefaultState : IInGameUIState
	{
		private readonly CharacterProvider _characterProvider;
		private readonly Hud _hud;
		private readonly InventoryView _inventoryView;
		
		private IDisposable _disposable;
		
		public DefaultState(CharacterProvider characterProvider, Hud hud, InventoryView inventoryView)
		{
			_characterProvider = characterProvider;
			_hud = hud;
			_inventoryView = inventoryView;
		}

		public virtual void Enter()
		{
			_disposable = _characterProvider.Character.Subscribe(OnCharacterChanged);
			Cursor.lockState = CursorLockMode.Locked;
			_hud.CanvasGroup.alpha = _characterProvider.Character.Value != null ? 1.0f : 0.0f;
		}

		public virtual void Exit()
		{
			_hud.CanvasGroup.alpha = 0.0f;
			_disposable.Dispose();
			_inventoryView.HideInventory();
		}

		private void OnCharacterChanged(Character character)
		{
			_hud.CanvasGroup.alpha = character != null ? 1.0f : 0.0f;
		}
	}
}