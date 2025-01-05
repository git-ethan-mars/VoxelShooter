using GamePlay.Data;

namespace Infrastructure.States
{
	public class StartSteamLobbyState : IPayloadedState<WorldSettings>
	{
		private readonly SceneLoader _sceneLoader;
		private const string Main = "Main";

		public StartSteamLobbyState(SceneLoader sceneLoader)
		{
			_sceneLoader = sceneLoader;
		}

		public void Enter(WorldSettings worldSettings)
		{
			_sceneLoader.Load(Main, () => CreateHost(worldSettings));
		}

		private void CreateHost(WorldSettings worldSettings)
		{
		}
		
		public void Exit()
		{
		}
	}
}