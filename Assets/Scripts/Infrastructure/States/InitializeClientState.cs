using Data;
using GamePlay;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;

namespace Infrastructure.States
{
	public class InitializeClientState : IState
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly VSNetworkManager _networkManager;
		private readonly GameModeFactory _gameModeFactory;

		public InitializeClientState(GameStateMachine gameStateMachine, VSNetworkManager networkManager, GameModeFactory gameModeFactory)
		{
			_gameStateMachine = gameStateMachine;
			_networkManager = networkManager;
			_gameModeFactory = gameModeFactory;
		}

		public async void Enter()
		{
			_networkManager.StartClient();

			await _networkManager.MessageReceived.FirstAsync<AuthenticationResponse>();
			NetworkClient.connection.isAuthenticated = true;

			_networkManager.SendRequest(new GameSettingsRequest());
			GameSettings gameSettings = (await _networkManager.MessageReceived
				.FirstAsync<GameSettingsResponse>()).Message.GameSettings;

			GameMode gameMode = await _gameModeFactory.CreateGameModeAsync(gameSettings);
			_gameStateMachine.Enter<GameLoopState, GameMode>(gameMode);
		}

		public void Exit()
		{
		}
	}
}
