namespace UI.Windows.States
{
    public class ChooseClassMenuState : IInGameUIState
    {
        private readonly ChooseClassMenu _chooseClassMenu;

        public ChooseClassMenuState(ChooseClassMenu chooseClassMenu)
        {
            _chooseClassMenu = chooseClassMenu;
        }

        public void Enter()
        {
            _chooseClassMenu.CanvasGroup.alpha = 1.0f;
        }

        public void Exit()
        {
            _chooseClassMenu.CanvasGroup.alpha = 0.0f;
        }
    }
}