using System;
using System.Collections.Generic;
using Common;
using Common.AssetManagement;
using Common.Factory;
using Common.StaticData;
using Common.Storage;
using Entities;
using Infrastructure.Factory;
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
		public event Action<GameObject> LocalPlayerCreated;
		public event Action GameFinished;

		private readonly NetworkManager _networkManager;
		private readonly IAssetProvider _assets;
		private readonly IStaticDataService _staticData;
		private readonly IParticleFactory _particleFactory;
		private readonly IEntityFactory _entityFactory;

		public MirrorClient(NetworkManager networkManager, IAssetProvider assets,
			IStaticDataService staticData, IStorageService storageService, IEntityFactory entityFactory,
			IParticleFactory particleFactory,
			IMeshFactory meshFactory)
		{
			_assets = assets;
			_staticData = staticData;
			_entityFactory = entityFactory;
			_particleFactory = particleFactory;
			_networkManager = networkManager;
			_soundMultiplier = storageService.Load<VolumeSettingsData>(Constants.VolumeSettingsKey).SoundVolume;
			_fallMeshGenerator = new FallMeshGenerator(particleFactory, meshFactory);
		}

		public void Start()
		{
			_networkManager.StartClient();
			RegisterMessageHandlers();
			RegisterPrefabs();
		}

		public void Stop()
		{
			_networkManager.StopClient();
			UnregisterMessageHandlers();
			UnregisterPrefabs();
			GameFinished?.Invoke();
		}

		public void ChangeClass(GameClass gameClass)
		{
			NetworkClient.Send(new ChangeClassRequest(gameClass));
		}

		private void RegisterMessageHandlers()
		{
			NetworkClient.RegisterHandler<MapNameResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<DownloadMapResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<UpdateMapResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<FallBlockResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<GameTimeResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<RespawnTimeResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<StartContinuousSoundResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<StopContinuousSoundResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<SurroundingSoundResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<PlayerSoundResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<RchParticleResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<StartMuzzleFlashResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<StopMuzzleFlashResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<ScoreboardResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<HealthResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<ChangeSlotResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<ItemUseResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<ShootResultResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<ReloadResultResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<DrillSpawnResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<DrillReloadResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<RocketSpawnResponse>(OnResponseReceived);
			NetworkClient.RegisterHandler<RocketReloadResponse>(OnResponseReceived);
		}

		private void RegisterPrefabs()
		{
			NetworkClient.RegisterPrefab(_assets.Load<GameObject>(EntityPath.MainPlayerPath), SpawnPlayerHandler, Object.Destroy);
			NetworkClient.RegisterPrefab(_assets.Load<GameObject>(EntityPath.SpectatorPlayerPath), SpawnSpectatorPlayer,
				Object.Destroy);
		}

		private GameObject SpawnPlayerHandler(SpawnMessage message)
		{
			var player = _entityFactory.CreateCharacter(message.position).gameObject;

			if (message.isLocalPlayer)
			{
				LocalPlayerCreated?.Invoke(player);
			}

			return player;
		}

		private GameObject SpawnSpectatorPlayer(SpawnMessage message)
		{
			return _entityFactory.CreateSpectatorPlayer(message.position).gameObject;
		}

		private void UnregisterPrefabs()
		{
			NetworkClient.UnregisterPrefab(_assets.Load<GameObject>(EntityPath.MainPlayerPath));
			NetworkClient.UnregisterPrefab(_assets.Load<GameObject>(EntityPath.SpectatorPlayerPath));
		}

		private void UnregisterMessageHandlers()
		{
			NetworkClient.UnregisterHandler<MapNameResponse>();
			NetworkClient.UnregisterHandler<DownloadMapResponse>();
			NetworkClient.UnregisterHandler<UpdateMapResponse>();
			NetworkClient.UnregisterHandler<FallBlockResponse>();
			NetworkClient.UnregisterHandler<GameTimeResponse>();
			NetworkClient.UnregisterHandler<RespawnTimeResponse>();
			NetworkClient.UnregisterHandler<StartContinuousSoundResponse>();
			NetworkClient.UnregisterHandler<StopContinuousSoundResponse>();
			NetworkClient.UnregisterHandler<SurroundingSoundResponse>();
			NetworkClient.UnregisterHandler<PlayerSoundResponse>();
			NetworkClient.UnregisterHandler<RchParticleResponse>();
			NetworkClient.UnregisterHandler<StartMuzzleFlashResponse>();
			NetworkClient.UnregisterHandler<StopMuzzleFlashResponse>();
			NetworkClient.UnregisterHandler<ScoreboardResponse>();
			NetworkClient.UnregisterHandler<HealthResponse>();
			NetworkClient.UnregisterHandler<ChangeSlotResponse>();
			NetworkClient.UnregisterHandler<ItemUseResponse>();
			NetworkClient.UnregisterHandler<ShootResultResponse>();
			NetworkClient.UnregisterHandler<ReloadResultResponse>();
			NetworkClient.UnregisterHandler<DrillSpawnResponse>();
			NetworkClient.UnregisterHandler<DrillReloadResponse>();
			NetworkClient.UnregisterHandler<RocketSpawnResponse>();
			NetworkClient.UnregisterHandler<RocketReloadResponse>();
		}
	}
}