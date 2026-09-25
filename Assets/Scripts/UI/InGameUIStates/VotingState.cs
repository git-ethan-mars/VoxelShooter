using Services;
using UnityEngine;

namespace UI.InGameUIStates
{
	// Map voting is modal: the cursor is free to click a map and the game does not react to input meanwhile.
	public class VotingState : IInGameUIState
	{
		private readonly IInputService _inputService;

		public VotingState(IInputService inputService)
		{
			_inputService = inputService;
		}

		public void Enter()
		{
			_inputService.Disable();
			Cursor.lockState = CursorLockMode.None;
		}

		public void Exit()
		{
			_inputService.Enable();
		}
	}
}
