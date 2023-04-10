using System.Linq;
using Data;
using Infrastructure.AssetManagement;
using Infrastructure.Services;
using Infrastructure.Services.Input;
using Inventory;
using MapLogic;
using Mirror;
using Networking;
using Networking.Synchronization;
using Player;
using Rendering;
using UI;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IMapProvider _mapProvider;
        private readonly IInputService _inputService;
        private readonly IStaticDataService _staticData;
        private MapGenerator _mapGenerator;
        private const string NetworkManagerPath = "Prefabs/Infrastructure/NetworkManager";
        private const string MapGeneratorPath = "Prefabs/MapCreation/MapGenerator";
        private const string ChunkRendererPath = "Prefabs/MapCreation/Chunk";
        private const string PlayerPath = "Prefabs/Player";
        private const string HudPath = "Prefabs/UI/HUD";


        public GameFactory(IAssetProvider assets, IMapProvider mapProvider, IInputService inputService,
            IStaticDataService staticData)
        {
            _staticData = staticData;
            _assets = assets;
            _mapProvider = mapProvider;
            _inputService = inputService;
        }
        

        public GameObject CreatePlayer(Vector3 position, Quaternion rotation) =>
            _assets.Instantiate(PlayerPath, position, rotation);

        public GameObject CreatePlayer() => _assets.Instantiate(PlayerPath);

        public GameObject InitializeLocalPlayer()
        {
            var player = NetworkClient.localPlayer.gameObject;
            var movement = player.GetComponent<PlayerMovement>();
            movement.Construct(_inputService, 10, 5);
            movement.enabled = true;
            var inventoryInput = player.GetComponent<InventoryInput>();
            inventoryInput.Construct(_inputService);
            inventoryInput.enabled = true;
            var mapSynchronization = player.GetComponent<MapSynchronization>();
            mapSynchronization.Construct(_mapProvider, AllServices.Container.Single<IMapGeneratorProvider>());
            player.GetComponent<RaycastSynchronization>().Construct(this);
            player.GetComponent<StatSynchronization>().Construct(player.GetComponent<HealthSystem>(),
                player.GetComponent<Player.Inventory>().inventory.OfType<PrimaryWeapon>().ToList());
            return player;
        }

        public GameObject CreateBulletHole(Vector3 position, Quaternion rotation)
        {
            var bullet = _assets.Instantiate(ParticlePath.BulletHolePath, position, rotation);
            NetworkServer.Spawn(bullet);
            return bullet;
        }

        public GameObject CreateMuzzleFlash(Transform transform) =>
            _assets.Instantiate(ParticlePath.MuzzleFlashPath, transform);

        public GameObject CreateNetworkManager()
        {
            var networkManager = _assets.Instantiate(NetworkManagerPath);
            networkManager.GetComponent<CustomNetworkManager>().Construct(_mapProvider, this, _staticData);
            return networkManager;
        }

        public GameObject CreateMapGenerator()
        {
            var mapGenerator = _assets.Instantiate(MapGeneratorPath);
            _mapGenerator = mapGenerator.GetComponent<MapGenerator>();
            _mapGenerator.Construct(_mapProvider, this);
            return mapGenerator;
        }

        public GameObject CreateHud(GameObject player)
        {
            var hud = _assets.Instantiate(HudPath);
            var inventoryController = hud.GetComponent<Hud>().inventory.GetComponent<InventoryController>();
            inventoryController.Construct(_assets, this, hud, player);
            return hud;
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