using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Inventory;
using UI;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class UIFactory : IUIFactory
    {
        private const string HudPath = "Prefabs/UI/HUD";
        private const string ChooseClassMenu = "Prefabs/UI/ChooseClassMenu";
        private readonly IAssetProvider _assets;
        private readonly IInputService _inputService;
        private IUIFactory _iuiFactoryImplementation;
        private readonly IEntityFactory _entityFactory;

        public UIFactory(IAssetProvider assets, IInputService inputService, IEntityFactory entityFactory)
        {
            _assets = assets;
            _inputService = inputService;
            _entityFactory = entityFactory;
        }
        
        public GameObject CreateHud(GameObject player)
        {
            var hud = _assets.Instantiate(HudPath);
            var inventoryController = hud.GetComponent<Hud>().inventory.GetComponent<InventoryController>();
            inventoryController.Construct(_inputService, _entityFactory, hud, player);
            hud.GetComponent<Hud>().healthCounter.Construct(player);
            return hud;
        }

        public void CreateChangeClassMenu()
        {
            _assets.Instantiate(ChooseClassMenu);
        }
    }
}