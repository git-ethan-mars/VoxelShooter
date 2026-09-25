using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay;
using Mirror;
using Networking;
using R3;

namespace Infrastructure.States
{
	public class InitializeHostState : IPayloadedState<GameSettings>
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly VSNetworkManager _networkManager;
		private readonly GameModeFactory _gameModeFactory;

		public InitializeHostState(GameStateMachine gameStateMachine, VSNetworkManager networkManager, GameModeFactory gameModeFactory)
		{
			_gameStateMachine = gameStateMachine;
			_networkManager = networkManager;
			_gameModeFactory = gameModeFactory;
		}

		public async UniTask EnterAsync(GameSettings gameSettings)
		{
			_networkManager.StartHost();
			// Subscribe before any await: authentication response may arrive while the map is loading.
			Task<Unit> clientAuthenticated = _networkManager.ClientAuthenticated.FirstAsync();

			GameMode gameMode = _gameModeFactory.CreateGameMode();
			await gameMode.StartAsync(gameSettings);

			await _gameStateMachine.EnterAsync<GameLoopState, GameMode>(gameMode);

			await gameMode.LoadMapAsync(gameSettings.MapName);

			// Server drops ReadyMessage from unauthenticated connections.
			await clientAuthenticated;
			NetworkClient.Ready();
		}

		public void Exit()
		{
		}
	}
}
