using Common;
using Common.AssetManagement;
using GamePlay.Services;
using UnityEngine;
using UnityEngine.UI;
using VoxelMap;

namespace UI
{
    public class UIFactory : IUIFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;

        public UIFactory(IAssetProvider assets, IStaticDataService staticData)
        {
            _assets = assets;
            _staticData = staticData;
        }

        public Hud CreateHud()
        {
            var hud = _assets.Instantiate(UIPath.HudPath).GetComponent<Hud>();
            return hud;
        }

        public MainMenu CreateMainMenu()
        {
            var mainMenu = _assets.Instantiate(UIPath.MainMenuPath).GetComponent<MainMenu>();
            return mainMenu;
        }

        public LoadingWindow CreateLoadingWindow()
        {
            return _assets.Instantiate(UIPath.LoadingWindowPath).GetComponent<LoadingWindow>();
        }

        public MatchMenu CreateMatchMenu(IMapRepository mapRepository)
        {
            var matchMenu = _assets.Instantiate(UIPath.MatchMenuPath).GetComponent<MatchMenu>();
            matchMenu.Construct(mapRepository, _staticData);
            return matchMenu;
        }

        public InGameUI CreateInGameUI(IInputService inputService, IStorageService storageService,
            IAvatarLoader avatarLoader)
        {
            var inGameUI = _assets.Instantiate(UIPath.InGameUIPath).GetComponent<InGameUI>();
            inGameUI.Construct(this, storageService, inputService, avatarLoader);
            return inGameUI;
        }

        public SettingsMenu CreateSettingsMenu(IStorageService storageService, Transform parent)
        {
            var settingsMenu = _assets.Instantiate(UIPath.SettingsMenuPath, parent).GetComponent<SettingsMenu>();
            settingsMenu.Construct(storageService);
            return settingsMenu;
        }

        public ChooseClassMenu CreateChooseClassMenu(Transform parent)
        {
            var chooseClassMenu = _assets.Instantiate(UIPath.ChooseClassMenuPath, parent).GetComponent<ChooseClassMenu>();
            chooseClassMenu.Construct();
            return chooseClassMenu;
        }

        public Scoreboard CreateScoreBoard(IAvatarLoader avatarLoader, Transform parent)
        {
            var scoreboard = _assets.Instantiate(UIPath.ScoreboardPath, parent).GetComponent<Scoreboard>();
            scoreboard.Construct(avatarLoader);
            return scoreboard;
        }

        public TimeCounter CreateTimeCounter(Transform parent)
        {
            var timeCounter = _assets.Instantiate(UIPath.TimeCounterPath, parent).GetComponent<TimeCounter>();
            timeCounter.Construct();
            return timeCounter;
        }

        public InGameMenu CreateInGameMenu(Transform parent)
        {
            var inGameMenu = _assets.Instantiate(UIPath.InGameMenuPath, parent).GetComponent<InGameMenu>();
            inGameMenu.Construct();
            return inGameMenu;
        }

        public Image CreateLootBoxImage(Transform parent)
        {
            return _assets.Instantiate(UIPath.LootBoxImagePath, parent).GetComponent<Image>();
        }
    }
}