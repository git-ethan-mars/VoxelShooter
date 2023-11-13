using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using Inventory;
using Networking;
using PlayerLogic;
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

        public Hud CreateHud(Player player, IClient client, IInputService inputService)
        {
            var hud = _assets.Instantiate(HudPath).GetComponent<Hud>();
            hud.Construct(inputService);
            var inventoryController = hud.inventory.GetComponent<InventoryController>();
            inventoryController.Construct(inputService, _assets, _staticData, hud, player);
            hud.healthCounter.Construct(client);
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

        public GameObject CreateMatchMenu(IMapRepository mapRepository, GameStateMachine gameStateMachine)
        {
            var matchMenu = _assets.Instantiate(MatchMenuPath);
            matchMenu.GetComponent<MatchMenu>().Construct(mapRepository, gameStateMachine);
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