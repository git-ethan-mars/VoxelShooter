using Cysharp.Threading.Tasks;
using Data;
using GamePlay;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;

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

		public async UniTask EnterAsync()
		{
			_networkManager.StartClient();

			await _networkManager.ClientAuthenticated.FirstAsync();
			GameSettings gameSettings = await GetGameSettings();
			GameMode gameMode = _gameModeFactory.CreateGameMode();
			await gameMode.StartAsync(gameSettings);

			await _gameStateMachine.EnterAsync<GameLoopState, GameMode>(gameMode);
		}

		public void Exit()
		{
		}

		private async UniTask<GameSettings> GetGameSettings()
		{
			_networkManager.SendRequest(new GameSettingsRequest());

			DirectedMessage<GameSettingsResponse> response = await _networkManager.MessageReceived
				.FirstAsync<GameSettingsResponse>();
			return response.Message.GameSettings;
		}
	}
}
