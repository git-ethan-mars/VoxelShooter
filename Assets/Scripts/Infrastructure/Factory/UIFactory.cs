using System;
using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Mirror;
using Networking;
using PlayerLogic;
using UI;
using UI.InGameUI;
using UI.SettingsMenu;
using UnityEngine;
using UnityEngine.UI;

namespace Infrastructure.Factory
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

        public Hud CreateHud(IClient client, Player player, IInputService inputService)
        {
            var hud = _assets.Instantiate(UIPath.HudPath).GetComponent<Hud>();
            hud.HealthCounter.Construct(player);
            hud.Palette.Construct(inputService);
            hud.Minimap.Construct(client, player, this);
            return hud;
        }

        public GameObject CreateMainMenu(GameStateMachine gameStateMachine)
        {
            var mainMenu = _assets.Instantiate(UIPath.MainMenuPath);
            mainMenu.GetComponent<MainMenu>().Construct(gameStateMachine);
            return mainMenu;
        }

        public void CreateLoadingWindow(CustomNetworkManager networkManager)
        {
            _assets.Instantiate(UIPath.LoadingWindowPath).GetComponent<LoadingWindow>().Construct(networkManager);
        }

        public GameObject CreateMatchMenu(GameStateMachine gameStateMachine, IMapRepository mapRepository)
        {
            var matchMenu = _assets.Instantiate(UIPath.MatchMenuPath);
            matchMenu.GetComponent<MatchMenu>().Construct(mapRepository, _staticData, gameStateMachine);
            return matchMenu;
        }

        public GameObject CreateSettingsMenu(IStorageService storageService, Action onBackButtonPressed)
        {
            var settingsMenu = _assets.Instantiate(UIPath.SettingsMenuPath);
            settingsMenu.GetComponent<SettingsMenu>().Construct(storageService, onBackButtonPressed);
            return settingsMenu;
        }

        public void CreateInGameUI(GameStateMachine gameStateMachine, CustomNetworkManager networkManager,
            IInputService inputService, IStorageService storageService,
            IAvatarLoader avatarLoader)
        {
            _assets.Instantiate(UIPath.InGameUIPath).GetComponent<InGameUI>()
                .Construct(gameStateMachine, networkManager, this, storageService, inputService, avatarLoader);
        }

        public ChooseClassMenu CreateChooseClassMenu(Transform parent)
        {
            var chooseClassMenu = _assets.Instantiate(UIPath.ChooseClassMenuPath, parent).GetComponent<ChooseClassMenu>();
            chooseClassMenu.Construct();
            return chooseClassMenu;
        }

        public Scoreboard CreateScoreBoard(Transform parent, CustomNetworkManager networkManager,
            IAvatarLoader avatarLoader)
        {
            var scoreboard = _assets.Instantiate(UIPath.ScoreboardPath, parent).GetComponent<Scoreboard>();
            scoreboard.Construct(networkManager, avatarLoader);
            return scoreboard;
        }

        public TimeCounter CreateTimeCounter(Transform parent, CustomNetworkManager networkManager)
        {
            var timeCounter = _assets.Instantiate(UIPath.TimeCounterPath, parent).GetComponent<TimeCounter>();
            timeCounter.Construct(networkManager);
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