using Infrastructure.AssetManagement;
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
        private readonly IInputService _inputService;
        private readonly IStaticDataService _staticData;
        private readonly IAvatarLoader _avatarLoader;


        public UIFactory(IAssetProvider assets, IInputService inputService, IStaticDataService staticData,
            IAvatarLoader avatarLoader)
        {
            _assets = assets;
            _inputService = inputService;
            _staticData = staticData;
            _avatarLoader = avatarLoader;
        }

        public GameObject CreateHud(GameObject player)
        {
            var hud = _assets.Instantiate(HudPath);
            hud.GetComponent<Hud>().Construct(_inputService);
            var inventoryController = hud.GetComponent<Hud>().inventory.GetComponent<InventoryController>();
            inventoryController.Construct(_inputService, _assets, _staticData, hud, player);
            hud.GetComponent<Hud>().healthCounter.Construct(player);
            return hud;
        }

        public void CreateChooseClassMenu(CustomNetworkManager networkManager, bool isLocalBuild)
        {
            _assets.Instantiate(ChooseClassMenuPath).GetComponent<ChooseClassMenu>().Construct(networkManager, _inputService, isLocalBuild);
        }

        public GameObject CreateMainMenu(GameStateMachine gameStateMachine, bool isLocalBuild)
        {
            var mainMenu = _assets.Instantiate(MainMenuPath);
            mainMenu.GetComponent<MainMenu>().Construct(gameStateMachine, isLocalBuild);
            return mainMenu;
        }

        public GameObject CreateMatchMenu(GameStateMachine gameStateMachine, bool isLocalBuild)
        {
            var matchMenu = _assets.Instantiate(MatchMenuPath);
            matchMenu.GetComponent<MatchMenu>().Construct(gameStateMachine, isLocalBuild);
            return matchMenu;
        }

        public void CreateTimeCounter(CustomNetworkManager networkManager)
        {
            _assets.Instantiate(TimeCounterPath).GetComponent<TimeCounter>().Construct(_inputService, networkManager);
        }

        public void CreateScoreboard(CustomNetworkManager networkManager)
        {
            _assets.Instantiate(ScoreboardPath).GetComponent<Scoreboard>().Construct(_inputService, _avatarLoader, networkManager);
        }

        public void CreateLoadingWindow(CustomNetworkManager networkManager)
        {
            _assets.Instantiate(LoadingWindowPath).GetComponent<LoadingWindow>().Construct(networkManager);
        }
    }
}