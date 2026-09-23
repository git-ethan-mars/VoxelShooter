using Data;
using GamePlay;
using Networking;

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

		public async void Enter(GameSettings gameSettings)
		{
			_networkManager.StartHost();
			GameMode gameMode = await _gameModeFactory.CreateGameMode(gameSettings);
			_gameStateMachine.Enter<GameLoopState, GameMode>(gameMode);
			await gameMode.LoadMapAsync(gameSettings.MapName);
		}

		public void Exit()
		{
		}
	}
}