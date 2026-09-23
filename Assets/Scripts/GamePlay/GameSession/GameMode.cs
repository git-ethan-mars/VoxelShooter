using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Data;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine;
using VoxelMap;

namespace GamePlay
{
	public abstract class GameMode
	{
		protected VSNetworkManager NetworkManager;
		protected EntityContainer EntityContainer;
		protected MapProvider MapProvider;

		protected readonly ReactiveProperty<GameState> MutableGameState = new ReactiveProperty<GameState>(GamePlay.GameState.Loading);
		private readonly List<Voxel> _addingVoxels = new List<Voxel>();
		private readonly List<Vector3Ushort> _removingPositions = new List<Vector3Ushort>();
		public GameSettings GameSettings { get; private set; }

		public ReadOnlyReactiveProperty<GameState> GameState => MutableGameState;

		public virtual void Update()
		{
			if (NetworkManager.mode == NetworkManagerMode.ClientOnly)
			{
				if (MapProvider.Map == null)
				{
					return;
				}

				MapProvider.Map.CurrentValue.SetVoxelsByGlobalPositions(_addingVoxels);
				_addingVoxels.Clear();

				Voxel[] removingVoxels = ArrayPool<Voxel>.Shared.Rent(_removingPositions.Count);

				for (int i = 0; i < _removingPositions.Count; i++)
				{
					removingVoxels[i] = new Voxel(_removingPositions[i], VoxelData.Air);
				}

				MapProvider.Map.CurrentValue.SetVoxelsByGlobalPositions(removingVoxels);
				_removingPositions.Clear();

				ArrayPool<Voxel>.Shared.Return(removingVoxels);
			}
		}

		public virtual async UniTask StartAsync(GameSettings gameSettings)
		{
			GameSettings = gameSettings;
			NetworkManager.PlayerConnected.Subscribe(tuple => OnAddPlayer(tuple.connection, tuple.nickName, tuple.avatar))
				.AddTo(NetworkManager);
			NetworkManager.PlayerDisconnected.Subscribe(OnRemovePlayer).AddTo(NetworkManager);
			NetworkManager.MessageReceived
				.OfMessageType<MapDownloadRequest>()
				.Subscribe(directedMessage =>
					NetworkManager.SendMapAsync(directedMessage.Connection).Forget())
				.AddTo(NetworkManager);

			if (NetworkManager.mode == NetworkManagerMode.ClientOnly)
			{
				NetworkManager.MessageReceived.OfMessageType<MapChangeResponse>()
					.Subscribe(_ => OnMapChangedAsync().Forget())
					.AddTo(NetworkManager);
				NetworkManager.MessageReceived.OfMessageType<AddedVoxelResponse>()
					.Subscribe(directedMessage => _addingVoxels.AddRange(directedMessage.Message.AddedVoxels))
					.AddTo(NetworkManager);
				NetworkManager.MessageReceived.OfMessageType<RemovedPositionResponse>()
					.Subscribe(directedMessage => _removingPositions.AddRange(directedMessage.Message.RemovedPositions))
					.AddTo(NetworkManager);

				await DownloadMapAsync(gameSettings.MapName);
			}
		}

		public virtual async UniTask LoadMapAsync(string mapName)
		{
			MapData mapData = await MapDataReader.ReadFromFileAsync(mapName);
			await MapProvider.LoadMapAsync(mapData, mapName);

			MapProvider.Map.CurrentValue.AddFeature<MapBuilding>();
			MapProvider.Map.CurrentValue.AddFeature<MapDestruction>();
			MapProvider.Map.CurrentValue.AddFeature<VoxelHealthSystem>();
			MapProvider.Map.CurrentValue.AddFeature<MapUpdateSender>();

			GameObjectInjector.InjectObject(MapProvider.Map.CurrentValue.gameObject,
				MapProvider.Map.CurrentValue.gameObject.scene.GetSceneContainer());
		}

		protected virtual void OnAddPlayer(NetworkConnectionToClient connectionToClient, string nickName, Texture2D avatar)
		{
		}

		protected virtual void OnRemovePlayer(NetworkConnectionToClient connection)
		{
		}

		protected virtual void CleanUp()
		{
			foreach (NetworkConnectionToClient connection in NetworkServer.connections.Values)
			{
				if (connection != NetworkServer.localConnection)
				{
					NetworkServer.SetClientNotReady(connection);
				}
			}

			foreach (Entity entity in EntityContainer.GetEntitiesByType<Entity>())
			{
				NetworkServer.Destroy(entity.gameObject);
			}
		}

		private async UniTask OnMapChangedAsync()
		{
			MutableGameState.Value = GamePlay.GameState.Loading;
			var mapNameRequest = new MapNameRequest();
			NetworkManager.SendRequest(mapNameRequest);
			DirectedMessage<MapNameResponse> mapNameResponse = await NetworkManager.MessageReceived.FirstAsync<MapNameResponse>();
			string mapName = mapNameResponse.Message.MapName;
			await DownloadMapAsync(mapName);
			MutableGameState.Value = GamePlay.GameState.Playing;
		}

		private async UniTask DownloadMapAsync(string mapName, CancellationToken cancellationToken = default)
		{
			var tsc = new UniTaskCompletionSource();
			var request = new MapDownloadRequest();
			NetworkManager.SendRequest(request);

			byte[] buffer = null;

			using IDisposable disposable = NetworkManager.MessageReceived
				.OfMessageType<MapDownloadResponse>()
				.Subscribe(response => ProcessMessageAsync(response.Message).Forget());

			async UniTask ProcessMessageAsync(MapDownloadResponse message)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					tsc.TrySetCanceled();
				}

				buffer ??= new byte[message.TotalBytes];

				Array.Copy(message.ByteChunk, 0, buffer, message.Offset, message.ByteChunk.Length);
				MapProvider.MapLoadingProgress?.Report(Mathf.Clamp01((float)(message.Offset + message.ByteChunk.Length) / message.TotalBytes));
				if (message.Offset + message.ByteChunk.Length >= message.TotalBytes)
				{
					using var memoryStream = new MemoryStream(buffer);
					MapData mapData = await MapDataReader.ReadFromStreamAsync(memoryStream, cancellationToken);
					await MapProvider.LoadMapAsync(mapData, mapName);
					tsc.TrySetResult();
				}
			}

			await tsc.Task.AttachExternalCancellation(cancellationToken);
		}
	}
}
