using Common.Factory;
using Common.StaticData;
using Infrastructure.Factory;
using Infrastructure.Services.PlayerDataLoader;
using MapLogic;
using Networking;
using Networking.Client;
using VoxelMap;

namespace Infrastructure.States
{
	public class StartMatchState : IPayloadedState<WorldSettings>
	{
		private readonly SceneLoader _sceneLoader;
		private readonly INetworkingFactory _networkingFactory;
		private readonly IMeshFactory _meshFactory;
		private readonly GameStateMachine _stateMachine;
		private IAuthenticationService _authenticationService;
		private const string Main = "Main";

		public StartMatchState(GameStateMachine stateMachine,
			SceneLoader sceneLoader, INetworkingFactory networkingFactory)
		{
			_stateMachine = stateMachine;
			_sceneLoader = sceneLoader;
			_networkingFactory = networkingFactory;
		}

		public void Enter(WorldSettings worldSettings)
		{
			_sceneLoader.Load(Main, () => CreateHost(worldSettings));
		}

		private void CreateHost(WorldSettings worldSettings)
		{
			var mapData = MapReader.ReadFromFile(worldSettings.MapName);
			var host = _networkingFactory.CreateHost(worldSettings);
			host.Start();
			_stateMachine.Enter<GameLoopState, (IClient, string, MapData)>((host.InnerClient, worldSettings.MapName, mapData));
		}

		public void Exit()
		{
		}
	}
}