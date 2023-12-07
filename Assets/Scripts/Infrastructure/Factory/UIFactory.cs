using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Networking;
using PlayerLogic;
using UI;
using UI.SettingsMenu;
using UI.Windows;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class UIFactory : IUIFactory
    {
        private const string HudPath = "Prefabs/UI/HUD";
        private const string ChooseClassMenuPath = "Prefabs/UI/ChooseClassMenu";
        private const string MainMenuPath = "Prefabs/UI/MainMenu";
        private const string MatchMenuPath = "Prefabs/UI/CreateMatchMenu";
        private const string TimeCounterPath = "Prefabs/UI/TimeInfo";
        private const string ScoreboardPath = "Prefabs/UI/Scoreboard";
        private const string LoadingWindowPath = "Prefabs/UI/LoadingWindow";
        private const string SettingsMenuPath = "Prefabs/UI/SettingsMenu";
        private const string InGameMenuPath = "Prefabs/UI/InGameMenu";
        private const string InGameUIPath = "Prefabs/UI/InGameUI";
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;


        public UIFactory(IAssetProvider assets, IStaticDataService staticData)
        {
            _assets = assets;
            _staticData = staticData;
        }

        public Hud CreateHud(Player player, IInputService inputService)
        {
            var hud = _assets.Instantiate(HudPath).GetComponent<Hud>();
            hud.healthCounter.Construct(player);
            hud.palette.Construct(inputService);
            return hud;
        }

        public GameObject CreateMainMenu(GameStateMachine gameStateMachine)
        {
            var mainMenu = _assets.Instantiate(MainMenuPath);
            mainMenu.GetComponent<MainMenu>().Construct(gameStateMachine);
            return mainMenu;
        }

        public void CreateLoadingWindow(IClient client)
        {
            _assets.Instantiate(LoadingWindowPath).GetComponent<LoadingWindow>().Construct(client);
        }

        public GameObject CreateMatchMenu(GameStateMachine gameStateMachine, IMapRepository mapRepository)
        {
            var matchMenu = _assets.Instantiate(MatchMenuPath);
            matchMenu.GetComponent<MatchMenu>().Construct(mapRepository, _staticData, gameStateMachine);
            return matchMenu;
        }

        public GameObject CreateSettingsMenu(GameStateMachine gameStateMachine, IStorageService storageService)
        {
            var settingsMenu = _assets.Instantiate(SettingsMenuPath);
            settingsMenu.GetComponent<SettingsMenu>().Construct(gameStateMachine, storageService);
            return settingsMenu;
        }

        public void CreateInGameUI(IClient client, IInputService inputService, IAvatarLoader avatarLoader)
        {
            _assets.Instantiate(InGameUIPath).GetComponent<InGameUI>()
                .Construct(this, inputService, client, avatarLoader);
        }

        public ChooseClassMenu CreateChooseClassMenu(Transform parent)
        {
            var chooseClassMenu = _assets.Instantiate(ChooseClassMenuPath, parent).GetComponent<ChooseClassMenu>();
            chooseClassMenu.Construct();
            return chooseClassMenu;
        }

        public Scoreboard CreateScoreBoard(Transform parent, IClient client, IAvatarLoader avatarLoader)
        {
            var scoreboard = _assets.Instantiate(ScoreboardPath, parent).GetComponent<Scoreboard>();
            scoreboard.Construct(client, avatarLoader);
            return scoreboard;
        }

        public TimeCounter CreateTimeCounter(Transform parent, IClient client)
        {
            var timeCounter = _assets.Instantiate(TimeCounterPath, parent).GetComponent<TimeCounter>();
            timeCounter.Construct(client);
            return timeCounter;
        }

        public InGameMenu CreateInGameMenu(Transform parent)
        {
            var inGameMenu = _assets.Instantiate(InGameMenuPath, parent).GetComponent<InGameMenu>();
            inGameMenu.Construct();
            return inGameMenu;
        }
    }
}