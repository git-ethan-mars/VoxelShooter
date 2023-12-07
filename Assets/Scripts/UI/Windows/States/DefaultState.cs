using Networking;

namespace UI.Windows.States
{
    public class DefaultState : IInGameUIState
    {
        private readonly TimeCounter _timeCounter;

        public DefaultState(TimeCounter timeCounter)
        {
            _timeCounter = timeCounter;
        }
        
        public void Enter()
        {
            _timeCounter.CanvasGroup.alpha = 1.0f;
        }

        public void Exit()
        {
            _timeCounter.CanvasGroup.alpha = 0.0f;
        }
    }
}