using Infrastructure.Services.Input;

namespace UI.InGameUI.States
{
    public class ScoreboardState : IInGameUIState
    {
        private readonly IInputService _inputService;
        private readonly Scoreboard _scoreboard;

        public ScoreboardState(IInputService inputService, Scoreboard scoreboard)
        {
            _inputService = inputService;
            _scoreboard = scoreboard;
        }

        public void Enter()
        {
            _inputService.Enable();
            _scoreboard.CanvasGroup.alpha = 1.0f;
        }

        public void Exit()
        {
            _inputService.Disable();
            _scoreboard.CanvasGroup.alpha = 0.0f;
        }
    }
}