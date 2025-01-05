using Common.AssetManagement;
using GamePlay;
using GamePlay.Services;
using Mirror;
using UnityEngine;

namespace Networking
{
    public class NetworkCharacterFactory : ICharacterFactory
    {
        private readonly IAssetProvider _assets;
        private readonly IStorageService _storageService;
        private readonly IInputService _inputService;
        private readonly IStaticDataService _staticData;

        public NetworkCharacterFactory(IAssetProvider assets, IStorageService storageService, IInputService inputService,
            IStaticDataService staticData)
        {
            _assets = assets;
            _storageService = storageService;
            _inputService = inputService;
            _staticData = staticData;
        }
        
        public Character CreateCharacter(Vector3 position)
        {
            var character = _assets.Instantiate(EntityPath.MainPlayerPath, position, Quaternion.identity)
                .GetComponent<Character>();
            character.Construct(_inputService, _storageService, _staticData);
            NetworkServer.Spawn(character.gameObject);
            return character;
        }

        public Spectator CreateSpectator(Vector3 position)
        {
            var spectator =
                _assets.Instantiate(EntityPath.SpectatorPlayerPath, position, Quaternion.identity).GetComponent<Spectator>();
            spectator.Construct(_inputService, _storageService);
            return spectator;
        }
    }
}