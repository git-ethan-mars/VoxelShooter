using System;
using GamePlay;
using R3;
using UnityEngine;
namespace UI.InGameUIStates
{
	public class DefaultState : IInGameUIState
	{
		private readonly CharacterProvider _characterProvider;
		
		private readonly TimeInfo _timeInfo;
		private readonly Hud _hud;
		
		private IDisposable _disposable;

		public DefaultState(CharacterProvider characterProvider, TimeInfo timeInfo, Hud hud)
		{
			_characterProvider = characterProvider;
			_timeInfo = timeInfo;
			_hud = hud;
		}

		public void Enter()
		{
			_disposable = _characterProvider.Character.Subscribe(OnCharacterChanged);
			Cursor.lockState = CursorLockMode.Locked;
			_timeInfo.CanvasGroup.alpha = 1.0f;
			_hud.CanvasGroup.alpha = _characterProvider.Character.Value != null ? 1.0f : 0.0f;
		}

		public void Exit()
		{
			_timeInfo.CanvasGroup.alpha = 0.0f;
			_hud.CanvasGroup.alpha = 0.0f;
			
			_disposable.Dispose();
		}

		private void OnCharacterChanged(Character character)
		{
			_hud.CanvasGroup.alpha = character != null ? 1.0f : 0.0f;
		}
	}
}