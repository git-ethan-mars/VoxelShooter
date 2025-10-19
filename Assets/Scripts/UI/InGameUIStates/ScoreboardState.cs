using Services;
using UnityEngine;
namespace UI.InGameUIStates
{
	public class ScoreboardState : IInGameUIState
	{
		private readonly ScoreboardView _scoreboard;

		public ScoreboardState(ScoreboardView scoreboard)
		{
			_scoreboard = scoreboard;
		}

		public void Enter()
		{
			Cursor.lockState = CursorLockMode.Locked;
			_scoreboard.CanvasGroup.alpha = 1.0f;
		}

		public void Exit()
		{
			_scoreboard.CanvasGroup.alpha = 0.0f;
		}
	}
}