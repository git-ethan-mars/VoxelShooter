using GamePlay;
using Networking;
using R3;
using UI;
namespace Infrastructure.States
{
	public class InitializeClientState : IState
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly GameSessionCreator _gameSessionCreator;
		private readonly VSNetworkManager _networkManager;

		private LoadingWindow _loadingWindow;

		public InitializeClientState(GameStateMachine gameStateMachine, GameSessionCreator gameSessionCreator,
			VSNetworkManager networkManager)
		{
			_gameStateMachine = gameStateMachine;
			_gameSessionCreator = gameSessionCreator;
			_networkManager = networkManager;
		}

		public async void Enter()
		{
			_networkManager.ClientDisconnected
				.Subscribe(_ => _gameStateMachine.Enter<GameMenuState>())
				.AddTo(_networkManager);
			GameSession gameSession = await _gameSessionCreator.ConnectToGameSession();
			_gameStateMachine.Enter<GameLoopState, GameSession>(gameSession);
		}

		public void Exit()
		{
		}
	}
}