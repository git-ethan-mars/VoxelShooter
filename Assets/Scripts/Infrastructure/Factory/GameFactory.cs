using Infrastructure.AssetManagement;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Inventory;
using MapLogic;
using Networking;
using Rendering;
using UI;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assets;
        private MapGenerator _mapGenerator;
        private readonly IInputService _inputService;
        private readonly IStaticDataService _staticData;
        private const string NetworkManagerPath = "Prefabs/Infrastructure/LocalNetworkManager";
        private const string SteamNetworkManagerPath = "Prefabs/Infrastructure/SteamManager";
        private const string MapGeneratorPath = "Prefabs/MapCreation/MapGenerator";
        private const string ChunkRendererPath = "Prefabs/MapCreation/Chunk";
        private const string HudPath = "Prefabs/UI/HUD";
        private const string ChangeClassMenu = "Prefabs/UI/Change Class Menu";
        private GameObject _networkManager;


        public GameFactory(IAssetProvider assets, IInputService inputService, IStaticDataService staticData)
        {
            _inputService = inputService;
            _assets = assets;
            _staticData = staticData;
        }

        public GameObject CreateLocalNetworkManager()
        {
            _networkManager = _assets.Instantiate(NetworkManagerPath);
            _networkManager.GetComponent<CustomNetworkManager>().Construct(_staticData);
            return _networkManager;
        }

        public GameObject CreateSteamNetworkManager()
        {
            _networkManager = _assets.Instantiate(SteamNetworkManagerPath);
            _networkManager.GetComponent<CustomNetworkManager>().Construct(_staticData);
            return _networkManager;
        }

        public GameObject CreateMapGenerator(Map map)
        {
            var mapGenerator = _assets.Instantiate(MapGeneratorPath);
            _mapGenerator = mapGenerator.GetComponent<MapGenerator>();
            _mapGenerator.Construct(map, this);
            return mapGenerator;
        }

        public GameObject CreateHud(GameObject player)
        {
            var hud = _assets.Instantiate(HudPath);
            var inventoryController = hud.GetComponent<Hud>().inventory.GetComponent<InventoryController>();
            inventoryController.Construct(_inputService, this, hud,player);
            hud.GetComponent<Hud>().healthBar.Construct(player);
            return hud;
        }

        public GameObject CreateGameModel(GameObject model, Transform itemPosition)
        {
            return Object.Instantiate(model, itemPosition);
        }

        public GameObject CreateChangeClassMenu()
        {
            var menu = _assets.Instantiate(ChangeClassMenu);
            menu.GetComponent<ChangeClassMenu>().Construct(_networkManager.GetComponent<CustomNetworkManager>()); 
            return menu;
        }

        public ChunkRenderer CreateChunkRenderer(Vector3Int position, Quaternion rotation, Transform transform)
        {
            var chunkRenderer = _assets.Instantiate(ChunkRendererPath, position, rotation, transform)
                .GetComponent<ChunkRenderer>();
            chunkRenderer.Construct();
            return chunkRenderer;
        }
    }
}