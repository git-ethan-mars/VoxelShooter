namespace UI
{
	public interface IUIFactory
	{
		GameMenu CreateGameMenu();
		InGameUI CreateInGameUI();
		LoadingWindow CreateLoadingWindow();
	}
}