using UnityEngine;

namespace UI.InGameUIStates
{
	public class ScoreboardState : IInGameUIState
	{
		private readonly DeathMatchScoreboardView _deathMatchScoreboard;

		public ScoreboardState(DeathMatchScoreboardView deathMatchScoreboard)
		{
			_deathMatchScoreboard = deathMatchScoreboard;
		}

		public void Enter()
		{
			Cursor.lockState = CursorLockMode.Locked;
			_deathMatchScoreboard.CanvasGroup.alpha = 1.0f;
		}

		public void Exit()
		{
			_deathMatchScoreboard.CanvasGroup.alpha = 0.0f;
		}
	}
}
