using Reflex.Extensions;
using Reflex.Injectors;
using Services;
namespace UI
{
	public class UIFactory : IUIFactory
	{
		private readonly IAssetProvider _assets;

		public UIFactory(IAssetProvider assets)
		{
			_assets = assets;
		}

		public GameMenu CreateGameMenu()
		{
			var gameMenu = _assets.Instantiate(UIPath.GameMenuPath).GetComponent<GameMenu>();
			GameObjectInjector.InjectRecursive(gameMenu.gameObject, gameMenu.gameObject.scene.GetSceneContainer());
			return gameMenu;
		}

		public InGameUI CreateInGameUI()
		{
			var inGameUI = _assets.Instantiate(UIPath.InGameUIPath).GetComponent<InGameUI>();
			GameObjectInjector.InjectRecursive(inGameUI.gameObject, inGameUI.gameObject.scene.GetSceneContainer());
			inGameUI.Hud.Initialize();
			inGameUI.Hud.InventoryPresenter.Initialize();
			inGameUI.ChooseClassMenu.Initialize();
			return inGameUI;
		}

		public LoadingWindow CreateLoadingWindow()
		{
			return _assets.Instantiate(UIPath.LoadingWindowPath).GetComponent<LoadingWindow>();
		}
	}
}