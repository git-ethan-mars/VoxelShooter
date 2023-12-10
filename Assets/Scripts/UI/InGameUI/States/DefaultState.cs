using Mirror;
using Networking;
using PlayerLogic;

namespace UI.InGameUI.States
{
    public class DefaultState : IInGameUIState
    {
        private readonly CustomNetworkManager _networkManager;
        private readonly TimeCounter _timeCounter;

        public DefaultState(CustomNetworkManager networkManager, TimeCounter timeCounter)
        {
            _networkManager = networkManager;
            _timeCounter = timeCounter;
        }

        public void Enter()
        {
            _timeCounter.CanvasGroup.alpha = 1.0f;
            _networkManager.Client.PlayerCreated += ShowHud;
            var identity = NetworkClient.connection.identity;
            if (identity != null && identity.TryGetComponent<Player>(out var player) && player.IsInitialized)
            {
                player.ShowHud();
            }
        }

        public void Exit()
        {
            _timeCounter.CanvasGroup.alpha = 0.0f;
            _networkManager.Client.PlayerCreated -= ShowHud;
            if (NetworkClient.connection is null)
            {
                return;
            }
            var identity = NetworkClient.connection.identity;
            if (identity != null && identity.TryGetComponent<Player>(out var player) && player.IsInitialized)
            {
                player.HideHud();
            }
        }

        private void ShowHud(Player player)
        {
            player.ShowHud();
        }
    }
}