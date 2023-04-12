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
        private readonly IStaticDataService _staticData;
        private MapGenerator _mapGenerator;
        private readonly IInputService _inputService;
        private const string NetworkManagerPath = "Prefabs/Infrastructure/NetworkManager";
        private const string MapGeneratorPath = "Prefabs/MapCreation/MapGenerator";
        private const string ChunkRendererPath = "Prefabs/MapCreation/Chunk";
        private const string PlayerPath = "Prefabs/Player";
        private const string HudPath = "Prefabs/UI/HUD";


        public GameFactory(IAssetProvider assets, IInputService inputService, IMapProvider mapProvider,
            IStaticDataService staticData)
        {
            _staticData = staticData;
            _inputService = inputService;
            _assets = assets;
            _mapProvider = mapProvider;
        }
        

        public GameObject CreatePlayer(GameClass gameClass, Vector3 position, Quaternion rotation)
        {
            var player = _assets.Instantiate(PlayerPath, position, rotation);
            ConfigurePlayer(player, gameClass);
            return player;
        }

        public GameObject CreatePlayer(GameClass gameClass)
        {
            var player = _assets.Instantiate(PlayerPath);
            ConfigurePlayer(player, gameClass);
            return player;
        }

        private void ConfigurePlayer(GameObject player, GameClass gameClass)
        {
            var characteristic = _staticData.GetPlayerCharacteristic(gameClass);
            var movement = player.GetComponent<PlayerMovement>();
            movement.Construct(characteristic);
            player.GetComponent<HealthSystem>().Construct(characteristic);
            var inventory = _staticData.GetInventory(gameClass).Select(item => item.id).ToArray();
            var playerInventory = player.GetComponent<Player.Inventory>();
            foreach (var itemId in inventory)
            {
                playerInventory.Ids.Add(itemId);
            }
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
            inventoryController.Construct(_assets, _inputService, this, hud, player);
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