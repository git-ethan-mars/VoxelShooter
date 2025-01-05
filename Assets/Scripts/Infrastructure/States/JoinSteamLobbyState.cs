using UI;

namespace Infrastructure.States
{
	public class JoinSteamLobbyState : IState
	{
		private readonly GameStateMachine _stateMachine;
		private readonly SceneLoader _sceneLoader;
		private readonly IUIFactory _uiFactory;
		private const string Main = "Main";

		public JoinSteamLobbyState(GameStateMachine stateMachine, SceneLoader sceneLoader, IUIFactory uiFactory)
		{
			_stateMachine = stateMachine;
			_sceneLoader = sceneLoader;
			_uiFactory = uiFactory;
		}

		public void Enter()
		{
			_sceneLoader.Load(Main, OnLoaded);
		}

		private void OnLoaded()
		{
		}

		public void Exit()
		{
		}
	}
}