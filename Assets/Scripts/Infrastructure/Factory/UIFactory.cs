﻿using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using Inventory;
using Networking;
using UI;
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
        private readonly IAssetProvider _assets;
        private readonly IStaticDataService _staticData;


        public UIFactory(IAssetProvider assets, IStaticDataService staticData)
        {
            _assets = assets;
            _staticData = staticData;
        }

        public GameObject CreateHud(IClient client, IInputService inputService, GameObject player)
        {
            var hud = _assets.Instantiate(HudPath);
            hud.GetComponent<Hud>().Construct(inputService);
            var inventoryController = hud.GetComponent<Hud>().inventory.GetComponent<InventoryController>();
            inventoryController.Construct(inputService, _assets, _staticData, hud, player);
            hud.GetComponent<Hud>().healthCounter.Construct(client);
            return hud;
        }

        public void CreateChooseClassMenu(IClient client, IInputService inputService, bool isLocalBuild)
        {
            _assets.Instantiate(ChooseClassMenuPath).GetComponent<ChooseClassMenu>()
                .Construct(client, inputService, isLocalBuild);
        }

        public GameObject CreateMainMenu(GameStateMachine gameStateMachine, bool isLocalBuild)
        {
            var mainMenu = _assets.Instantiate(MainMenuPath);
            mainMenu.GetComponent<MainMenu>().Construct(gameStateMachine, isLocalBuild);
            return mainMenu;
        }

        public GameObject CreateMatchMenu(IMapRepository mapRepository, GameStateMachine gameStateMachine,
            bool isLocalBuild)
        {
            var matchMenu = _assets.Instantiate(MatchMenuPath);
            matchMenu.GetComponent<MatchMenu>().Construct(mapRepository, gameStateMachine, isLocalBuild);
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
    }
}