using Cysharp.Threading.Tasks;

namespace Infrastructure.States
{
	public class BootstrapState : IState
	{
		private readonly SceneLoader _sceneLoader;
		private readonly GameStateMachine _stateMachine;

		public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader)
		{
			_stateMachine = stateMachine;
			_sceneLoader = sceneLoader;
		}

		public async UniTask EnterAsync()
		{
			await _sceneLoader.LoadAsync(Scenes.Initial);
			await _stateMachine.EnterAsync<GameMenuState>();
		}

		public void Exit()
		{
		}
	}
}
