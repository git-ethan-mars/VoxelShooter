using System;
using R3;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI.SettingsMenuStates
{
	public class GameplaySettingsState : ISettingsMenuState
	{
		private readonly Toggle _autoJumpToggle;
		private readonly Toggle _autoReloadToggle;
		private readonly GameObject _gameplaySection;
		private readonly IStorageService _storageService;

		private IDisposable _disposable;

		public GameplaySettingsState(IStorageService storageService, GameObject gameplaySection, Toggle autoJumpToggle,
			Toggle autoReloadToggle)
		{
			_storageService = storageService;
			_gameplaySection = gameplaySection;
			_autoJumpToggle = autoJumpToggle;
			_autoReloadToggle = autoReloadToggle;

			GameplaySettingsData currentSettings =
				_storageService.Load<GameplaySettingsData>(IStorageService.GameplaySettingsKey);
			_autoJumpToggle.SetIsOnWithoutNotify(currentSettings.AutoJump);
			_autoReloadToggle.SetIsOnWithoutNotify(currentSettings.AutoReload);
		}

		public void Enter()
		{
			_gameplaySection.SetActive(true);

			_disposable = Observable.Merge(_autoJumpToggle.onValueChanged.AsObservable(),
					_autoReloadToggle.onValueChanged.AsObservable())
				.Subscribe(_ => _storageService.Set(new GameplaySettingsData(_autoJumpToggle.isOn, _autoReloadToggle.isOn)));
		}

		public void Exit()
		{
			_disposable.Dispose();
			_gameplaySection.SetActive(false);
		}
	}
}
