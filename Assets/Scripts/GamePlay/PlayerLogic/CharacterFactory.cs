using Common.AssetManagement;
using GamePlay.Services;
using UnityEngine;

namespace GamePlay
{
    public class CharacterFactory : ICharacterFactory
    {
        private const string MainPlayerPath = "Prefabs/Player";
        private const string SpectatorPlayerPath = "Prefabs/Spectator player";

        private readonly IAssetProvider _assets;
        private readonly IStorageService _storageService;
        private readonly IInputService _inputService;
        private readonly IStaticDataService _staticData;

        public CharacterFactory(IAssetProvider assets, IStorageService storageService, IInputService inputService,
            IStaticDataService staticData)
        {
            _assets = assets;
            _storageService = storageService;
            _inputService = inputService;
            _staticData = staticData;
        }

        public Character CreateCharacter(Vector3 position)
        {
            var character = _assets.Instantiate(MainPlayerPath, position, Quaternion.identity)
                .GetComponent<Character>();
            character.Construct(_inputService, _storageService, _staticData);
            return character;
        }

        public Spectator CreateSpectator(Vector3 position)
        {
            var spectator =
                _assets.Instantiate(SpectatorPlayerPath, position, Quaternion.identity).GetComponent<Spectator>();
            spectator.Construct(_inputService, _storageService);
            return spectator;
        }
    }
}