using Common.Input;
using UnityEngine;

namespace UI.InGameUIStates
{
    public class DefaultState : IInGameUIState
    {
        private readonly IInputService _inputService;
        private readonly TimeCounter _timeCounter;

        public DefaultState(IInputService inputService, TimeCounter timeCounter)
        {
            _inputService = inputService;
            _timeCounter = timeCounter;
        }

        public void Enter()
        {
            Cursor.lockState = CursorLockMode.Locked;
            _inputService.Enable();
            _timeCounter.CanvasGroup.alpha = 1.0f;
        }

        public void Exit()
        {
            _inputService.Disable();
            _timeCounter.CanvasGroup.alpha = 0.0f;
        }
    }
}