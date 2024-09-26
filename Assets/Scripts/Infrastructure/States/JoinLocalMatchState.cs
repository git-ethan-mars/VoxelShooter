using Infrastructure.Factory;
using Networking;
using Networking.Client;
using UI;
using UnityEngine;
using VoxelMap;

namespace Infrastructure.States
{
	public class JoinLocalMatchState : IState
	{
		private const string Main = "Main";

		private readonly GameStateMachine _stateMachine;
		private readonly SceneLoader _sceneLoader;
		private readonly INetworkingFactory _networkingFactory;
		private readonly IUIFactory _uiFactory;
		private IClient _client;
		private LoadingWindow _loadingWindow;


		public JoinLocalMatchState(GameStateMachine stateMachine, SceneLoader sceneLoader, INetworkingFactory networkingFactory, IUIFactory uiFactory)
		{
			_stateMachine = stateMachine;
			_sceneLoader = sceneLoader;
			_networkingFactory = networkingFactory;
			_uiFactory = uiFactory;
		}


		public void Enter()
		{
			_sceneLoader.Load(Main, OnLoaded);
		}

		private void OnLoaded()
		{
			_loadingWindow = _uiFactory.CreateLoadingWindow();
			_client = _networkingFactory.CreateClient();
			_client.MapLoaded += OnMapLoaded;
			_client.MapLoadProgressed += _loadingWindow.UpdateLoadingBar;
			_client.Start();
		}

		private void OnMapLoaded(string mapName, MapData mapData)
		{
			_client.MapLoaded -= OnMapLoaded;
			_client.MapLoadProgressed -= _loadingWindow.UpdateLoadingBar;
			Object.Destroy(_loadingWindow);
			_stateMachine.Enter<GameLoopState, (IClient, string, MapData)>((_client, mapName, mapData));
		}

		public void Exit()
		{
		}
	}
}