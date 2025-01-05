using System;
using System.Collections.Generic;
using Common.AssetManagement;
using Cysharp.Threading.Tasks;
using GamePlay;
using GamePlay.Data;
using GamePlay.Factory;
using GamePlay.MapFeatures;
using GamePlay.Services;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using UnityEngine;
using VoxelMap;
using Object = UnityEngine.Object;

namespace Networking.Client
{
	public sealed partial class MirrorClient : IClient
	{
		public event Action<string, MapData> MapLoaded;
		public event Action<float> MapLoadProgressed;
		public event Action<ServerTime> GameTimeChanged;
		public event Action<ServerTime> RespawnTimeChanged;
		public event Action<List<ScoreData>> ScoreboardChanged;
		public event Action<Character> CharacterSpawned;
		public event Action<Character> CharacterDespawned;
		public event Func<UniTask> GameFinished;

		private readonly NetworkManager _networkManager;
		private readonly IStaticDataService _staticData;
		private readonly ICharacterFactory _characterFactory;
		private readonly IInventoryFactory _inventoryFactory;
		private readonly GameObject _characterPrefab;

		public MirrorClient(NetworkManager networkManager, IAssetProvider assets,
			IStaticDataService staticData, IStorageService storageService, ICharacterFactory characterFactory,
			IParticleFactory particleFactory,
			IMeshFactory meshFactory, IInventoryFactory inventoryFactory)
		{
			_staticData = staticData;
			_characterFactory = characterFactory;
			_inventoryFactory = inventoryFactory;
			_networkManager = networkManager;
			_soundMultiplier = storageService.Load<VolumeSettingsData>(IStorageService.VolumeSettingsKey).SoundVolume;
			_fallMeshGenerator = new FallMeshGenerator(particleFactory, meshFactory);
			_characterPrefab = assets.Load<GameObject>(EntityPath.MainPlayerPath);
		}

		public void Start()
		{
			if (!NetworkClient.active)
			{
				_networkManager.StartClient();
			}
			
			RegisterMessageHandlers();
			NetworkClient.RegisterPrefab(_characterPrefab, SpawnCharacterHandler, DespawnCharacterHandler);
		}

		private GameObject SpawnCharacterHandler(SpawnMessage message)
		{
			var character = _characterFactory.CreateCharacter(message.position);
			var playerData = character.GetComponent<IPlayerData>();
			var inventory = _inventoryFactory.CreateInventory(playerData.GameClass, _mapProvider);
			character.Initialize(inventory);
			if (message.isLocalPlayer)
			{
				CharacterSpawned?.Invoke(character);
			}

			return character.gameObject;
		}

		private void DespawnCharacterHandler(GameObject characterGameObject)
		{
			var netIdentity = characterGameObject.GetComponent<NetworkIdentity>();
			if (netIdentity.isLocalPlayer)
			{
				var character = characterGameObject.GetComponent<Character>();
				CharacterDespawned?.Invoke(character);
			}
			
			Object.Destroy(characterGameObject);
		}

		public void Stop()
		{
			_networkManager.StopClient();
			UnregisterMessageHandlers();
			NetworkClient.UnregisterPrefab(_characterPrefab);
			GameFinished?.Invoke();
		}

		public void ChangeClass(GameClass gameClass)
		{
			NetworkClient.Send(new ChangeClassRequest(gameClass));
		}

		private void RegisterMessageHandlers()
		{
			NetworkClient.RegisterHandler<AuthenticationResponse>(OnResponseReceived, false);
			NetworkClient.RegisterHandler<MapNameResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<DownloadMapResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<UpdateMapResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<GameTimeResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<RespawnTimeResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<StartContinuousSoundResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<StopContinuousSoundResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<SurroundingSoundResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<PlayerSoundResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<ScoreboardResponse>(OnResponseReceived);
		}

		private void UnregisterMessageHandlers()
		{
			NetworkClient.UnregisterHandler<AuthenticationResponse>();
			NetworkClient.UnregisterHandler<MapNameResponse>();
			NetworkClient.UnregisterHandler<DownloadMapResponse>();
			NetworkClient.UnregisterHandler<UpdateMapResponse>();
			NetworkClient.UnregisterHandler<GameTimeResponse>();
			NetworkClient.UnregisterHandler<RespawnTimeResponse>();
			NetworkClient.UnregisterHandler<StartContinuousSoundResponse>();
			NetworkClient.UnregisterHandler<StopContinuousSoundResponse>();
			NetworkClient.UnregisterHandler<SurroundingSoundResponse>();
			NetworkClient.UnregisterHandler<PlayerSoundResponse>();
			NetworkClient.UnregisterHandler<ScoreboardResponse>();
		}
	}
}