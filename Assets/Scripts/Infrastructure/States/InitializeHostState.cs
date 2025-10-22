using Data;
using GamePlay;
using UI;
namespace Infrastructure.States
{
	public class InitializeHostState : IPayloadedState<GameSettings>
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly GameSessionCreator _gameSessionCreator;

		private LoadingWindow _loadingWindow;

		public InitializeHostState(GameStateMachine gameStateMachine, GameSessionCreator gameSessionCreator)
		{
			_gameStateMachine = gameStateMachine;
			_gameSessionCreator = gameSessionCreator;
		}

		public void Enter(GameSettings gameSettings)
		{
			GameSession gameSession = _gameSessionCreator.Create(gameSettings);
			
			_gameStateMachine.Enter<GameLoopState, GameSession>(gameSession);
		}

		public void Exit()
		{
		}
	}
}