﻿using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using Infrastructure.States;
using Networking;
using PlayerLogic;
using UI;
using UI.SettingsMenu;
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
            hud.Construct(inputService);
            hud.healthCounter.Construct(player);
            hud.palette.Construct(inputService);
            return hud;
        }

        public void CreateChooseClassMenu(IClient client, IInputService inputService)
        {
            _assets.Instantiate(ChooseClassMenuPath).GetComponent<ChooseClassMenu>()
                .Construct(client, inputService);
        }

        public GameObject CreateMainMenu(GameStateMachine gameStateMachine)
        {
            var mainMenu = _assets.Instantiate(MainMenuPath);
            mainMenu.GetComponent<MainMenu>().Construct(gameStateMachine);
            return mainMenu;
        }

        public GameObject CreateMatchMenu(GameStateMachine gameStateMachine, IMapRepository mapRepository)
        {
            var matchMenu = _assets.Instantiate(MatchMenuPath);
            matchMenu.GetComponent<MatchMenu>().Construct(mapRepository, _staticData, gameStateMachine);
            return matchMenu;
        }

        public void CreateTimeCounter(IClient client, IInputService inputService)
        {
            _assets.Instantiate(TimeCounterPath).GetComponent<TimeCounter>().Construct(client, inputService);
        }

        public void CreateScoreboard(IClient client, IInputService inputService, IAvatarLoader avatarLoader)
        {
            _assets.Instantiate(ScoreboardPath).GetComponent<Scoreboard>()
                .Construct(client, inputService, avatarLoader);
        }

        public void CreateLoadingWindow(IClient client)
        {
            _assets.Instantiate(LoadingWindowPath).GetComponent<LoadingWindow>().Construct(client);
        }

        public GameObject CreateSettingsMenu(GameStateMachine gameStateMachine, IStorageService storageService)
        {
            var settingsMenu = _assets.Instantiate(SettingsMenuPath);
            settingsMenu.GetComponent<SettingsMenu>().Construct(gameStateMachine, storageService);
            return settingsMenu;
        }
    }
}