using System;
using GamePlay;
using Reflex.Extensions;
using Reflex.Injectors;
using Services;

namespace UI
{
	public class UIFactory : IUIFactory
	{
		private readonly IAssetProvider _assets;
		private readonly UIProvider _uiProvider;

		public UIFactory(IAssetProvider assets, UIProvider uiProvider)
		{
			_assets = assets;
			_uiProvider = uiProvider;
		}

		public GameMenu CreateGameMenu()
		{
			GameMenu gameMenu = _assets.Instantiate(UIPath.GameMenuPath).GetComponent<GameMenu>();
			GameObjectInjector.InjectRecursive(gameMenu.gameObject, gameMenu.gameObject.scene.GetSceneContainer());
			return gameMenu;
		}

		public GameModeView CreateGameModeView(GameMode gameMode)
		{
			if (gameMode is DeathMatch deathMatch)
			{
				DeathMatchView view = _assets.Instantiate(UIPath.DeathMatchViewPath).GetComponent<DeathMatchView>();
				view.Initialize(deathMatch);
				_uiProvider.Hud = view.Hud;
				return view;
			}

			throw new ArgumentException(nameof(gameMode));
		}

		public LoadingWindow CreateLoadingWindow()
		{
			LoadingWindow loadingWindow = _assets.Instantiate(UIPath.LoadingWindowPath).GetComponent<LoadingWindow>();
			_uiProvider.LoadingWindow = loadingWindow;
			return loadingWindow;
		}
	}
}
