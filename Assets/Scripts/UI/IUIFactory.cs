using GamePlay;

namespace UI
{
	public interface IUIFactory
	{
		GameMenu CreateGameMenu();
		LoadingWindow CreateLoadingWindow();
		GameModeView CreateGameModeView(GameMode gameMode);
	}
}
