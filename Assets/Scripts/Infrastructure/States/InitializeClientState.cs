using GamePlay;
using UI;
namespace Infrastructure.States
{
	public class InitializeClientState : IState
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly GameSessionCreator _gameSessionCreator;

		private LoadingWindow _loadingWindow;

		public InitializeClientState(GameStateMachine gameStateMachine, GameSessionCreator gameSessionCreator)
		{
			_gameStateMachine = gameStateMachine;
			_gameSessionCreator = gameSessionCreator;
		}

		public async void Enter()
		{
			GameSession gameSession = await _gameSessionCreator.ConnectToGameSession();
			_gameStateMachine.Enter<GameLoopState, GameSession>(gameSession);
		}

		public void Exit()
		{
		}
	}
}