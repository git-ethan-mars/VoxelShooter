using Services;

namespace UI.InGameUIStates
{
	// Chat is typed over the game screen, so the underlying state stays visible.
	public class ChatState : IInGameUIState
	{
		private readonly IInGameUIState _gameState;
		private readonly IInputService _inputService;
		private readonly ChatView _chatView;

		public ChatState(IInGameUIState gameState, IInputService inputService, ChatView chatView)
		{
			_gameState = gameState;
			_inputService = inputService;
			_chatView = chatView;
		}

		public void Enter()
		{
			_gameState.Enter();
			_inputService.Disable();
			_chatView.Open();
		}

		public void Exit()
		{
			_chatView.Close();
			_inputService.Enable();
			_gameState.Exit();
		}
	}
}
