using Common;
using Common.Input;
using Common.Storage;
using UI;
using UnityEngine;

namespace Infrastructure.Factory
{
	public interface IUIFactory : IService
	{
		MainMenu CreateMainMenu();
		MatchMenu CreateMatchMenu(IMapRepository mapRepository);
		SettingsMenu CreateSettingsMenu(IStorageService storageService, Transform parent);
		LoadingWindow CreateLoadingWindow();

		InGameUI CreateInGameUI(IInputService inputService, IStorageService storageService,
			IAvatarLoader avatarLoader);

		ChooseClassMenu CreateChooseClassMenu(Transform parent);
		Scoreboard CreateScoreBoard(IAvatarLoader avatarLoader, Transform parent);
		TimeCounter CreateTimeCounter(Transform parent);
		InGameMenu CreateInGameMenu(Transform parent);
	}
}