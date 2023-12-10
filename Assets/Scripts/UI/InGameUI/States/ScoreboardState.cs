namespace UI.InGameUI.States
{
    public class ScoreboardState : IInGameUIState
    {
        private readonly Scoreboard _scoreboard;

        public ScoreboardState(Scoreboard scoreboard)
        {
            _scoreboard = scoreboard;
        }

        public void Enter()
        {
            _scoreboard.CanvasGroup.alpha = 1.0f;
        }

        public void Exit()
        {
            _scoreboard.CanvasGroup.alpha = 0.0f;
        }
    }
}