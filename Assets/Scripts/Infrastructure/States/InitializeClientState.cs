using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Data;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;
using Services;
using UI;
using UnityEngine;
using VoxelMap;
using Object = UnityEngine.Object;
namespace Infrastructure.States
{
	public class InitializeClientState : IState
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly IMapFactory _mapFactory;
		private readonly IUIFactory _uiFactory;
		private readonly IMapConfigureLoader _mapConfigureLoader;
		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly MapDownloader _mapDownloader;
		private readonly MapProvider _mapProvider;

		private LoadingWindow _loadingWindow;
		private CancellationTokenSource _cts;
		private IDisposable _clientStopped;

		public InitializeClientState(GameStateMachine gameStateMachine, IMapFactory mapFactory,
			IUIFactory uiFactory, IMapConfigureLoader mapConfigureLoader, VoxelShooterNetworkManager networkManager,
			MapDownloader mapDownloader, MapProvider mapProvider)
		{
			_gameStateMachine = gameStateMachine;
			_mapFactory = mapFactory;
			_uiFactory = uiFactory;
			_mapConfigureLoader = mapConfigureLoader;
			_networkManager = networkManager;
			_mapDownloader = mapDownloader;
			_mapProvider = mapProvider;
		}

		public async void Enter()
		{
			_cts = new CancellationTokenSource();
			_loadingWindow = _uiFactory.CreateLoadingWindow();
			_clientStopped = _networkManager.ClientStopped.Subscribe(_ => OnClientDisconnected());
			await StartClient();
			
			if (_cts.IsCancellationRequested)
			{
				return;
			}
			
			await DownloadMap();
			_networkManager.authenticator.OnClientAuthenticated.Invoke();
			_gameStateMachine.Enter<GameLoopState>();
		}

		public void Exit()
		{
			_cts.Dispose();
			_clientStopped.Dispose();
			Object.Destroy(_loadingWindow.gameObject);
		}

		private async UniTask DownloadMap()
		{
			var progress = new Progress<float>(OnMapLoadingProgressed);
			string mapName = await _mapDownloader.DownloadMapNameAsync(_cts.Token);
			MapData mapData = await _mapDownloader.DownloadMapAsync(progress, _cts.Token);

			MapConfigure configure = _mapConfigureLoader.GetMapConfigure(mapName);
			Map map = await CreateMap(mapData, configure, progress);

			_mapProvider.MapName = mapName;
			_mapProvider.Confgure = configure;
			_mapProvider.Map = map;
		}

		private async UniTask StartClient()
		{
			_networkManager.StartClient();
			await _networkManager.MessageReceived
				.OfMessageType<AuthenticationResponse>()
				.FirstAsync(_cts.Token)
				.AsUniTask()
				.SuppressCancellationThrow();
		}

		private void OnClientDisconnected()
		{
			_cts.Cancel();
			_gameStateMachine.Enter<GameMenuState>();
		}

		private async Task<Map> CreateMap(MapData mapData, MapConfigure configure, IProgress<float> progress)
		{
			MapBuilder mapBuilder = new MapBuilder(_mapFactory, mapData).FromConfigure(configure);
			Map map = await mapBuilder.BuildAsync(progress);
			return map;
		}

		private void OnMapLoadingProgressed(float value)
		{
			_loadingWindow.UpdateLoadingBar(value);
		}
	}
}