using Infrastructure.AssetManagement;
using Infrastructure.Services.Input;
using Infrastructure.Services.StaticData;
using Infrastructure.Services.Storage;
using PlayerLogic;
using UnityEngine;

namespace Infrastructure.Factory
{
    public class PlayerFactory : IPlayerFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IInputService _inputService;
        private readonly IStorageService _storageService;
        private readonly IStaticDataService _staticData;
        private readonly IUIFactory _uiFactory;
        private readonly IMeshFactory _meshFactory;

        public PlayerFactory(IAssetProvider assets, IInputService inputService, IStorageService storageService,
            IStaticDataService staticData, IUIFactory uiFactory, IMeshFactory meshFactory)
        {
            _assets = assets;
            _inputService = inputService;
            _storageService = storageService;
            _staticData = staticData;
            _uiFactory = uiFactory;
            _meshFactory = meshFactory;
        }

        public GameObject CreatePlayer(Vector3 position)
        {
            var player = _assets.Instantiate(PlayerPath.MainPlayerPath, position, Quaternion.identity);
            player.GetComponent<Player>()
                .Construct(_inputService, _storageService, _staticData, _uiFactory, _meshFactory);
            return player;
        }

        public GameObject CreateSpectatorPlayer(Vector3 deathPosition)
        {
            var spectator = _assets.Instantiate(PlayerPath.SpectatorPlayerPath, deathPosition, Quaternion.identity);
            return spectator;
        }
    }
}