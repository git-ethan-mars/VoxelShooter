using Infrastructure.AssetManagement;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Infrastructure.States;
using Inventory;
using UI;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class UIFactory : IUIFactory
    {
        private const string HudPath = "Prefabs/UI/HUD";
        private const string ChooseClassMenu = "Prefabs/UI/ChooseClassMenu";
        private const string MainMenu = "Prefabs/UI/MainMenu";
        private const string MatchMenu = "Prefabs/UI/CreateMatchMenu";
        private readonly IAssetProvider _assets;
        private readonly IInputService _inputService;
        private IUIFactory _iuiFactoryImplementation;
        private readonly IStaticDataService _staticData;

        public UIFactory(IAssetProvider assets, IInputService inputService, IStaticDataService staticData)
        {
            _assets = assets;
            _inputService = inputService;
            _staticData = staticData;
        }
        
        public GameObject CreateHud(GameObject player)
        {
            var hud = _assets.Instantiate(HudPath);
            var inventoryController = hud.GetComponent<Hud>().inventory.GetComponent<InventoryController>();
            inventoryController.Construct(_inputService, _staticData, hud, player);
            hud.GetComponent<Hud>().healthCounter.Construct(player);
            return hud;
        }

        public void CreateChangeClassMenu()
        {
            _assets.Instantiate(ChooseClassMenu);
        }

        public GameObject CreateMainMenu(GameStateMachine gameStateMachine, bool isLocalBuild)
        {
            var mainMenu = _assets.Instantiate(MainMenu);
            mainMenu.GetComponent<MainMenu>().Construct(gameStateMachine, isLocalBuild);
            return mainMenu;
        }

        public void CreateMatchMenu(GameStateMachine gameStateMachine, bool isLocalBuild)
        {
            var matchMenu = _assets.Instantiate(MatchMenu);
            matchMenu.GetComponent<MatchMenu>().Construct(gameStateMachine, isLocalBuild);
        }
    }
}