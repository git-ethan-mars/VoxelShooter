namespace UI.Windows.States
{
    public class InGameMenuState : IInGameUIState
    {
        private readonly InGameMenu _inGameMenu;

        public InGameMenuState(InGameMenu inGameMenu)
        {
            _inGameMenu = inGameMenu;
        }

        public void Enter()
        {
            _inGameMenu.CanvasGroup.alpha = 1.0f;
        }

        public void Exit()
        {
            _inGameMenu.CanvasGroup.alpha = 0.0f;
        }
    }
}