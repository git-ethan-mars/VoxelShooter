namespace UI.InGameUIStates
{
	public class WorldMapState : IInGameUIState
	{
		private readonly WorldMap _worldMap;

		public WorldMapState(WorldMap worldMap)
		{
			_worldMap = worldMap;
		}
		
		public void Enter()
		{
			_worldMap.CanvasGroup.alpha = 1;
		}

		public void Exit()
		{
			_worldMap.CanvasGroup.alpha = 0;
		}
	}
}