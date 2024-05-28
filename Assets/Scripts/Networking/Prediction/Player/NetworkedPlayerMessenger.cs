using System;
using Mirror;

namespace Networking.Prediction.Player
{
    public class NetworkedPlayerMessenger : NetworkBehaviour, INetworkedClientMessenger<PlayerInput, PlayerState>
    {
        public event Action<PlayerInput> OnInputReceived;
        public PlayerState LatestServerState { get; private set; }

        public void SendState(PlayerState state)
        {
            RpcSendState(state);
        }

        public void SendInput(PlayerInput input)
        {
            CmdSendInput(input);
        }

        [ClientRpc(channel = Channels.Unreliable)]
        void RpcSendState(PlayerState state)
        {
            LatestServerState = state;
        }

        [Command(channel = Channels.Unreliable)]
        void CmdSendInput(PlayerInput input)
        {
            OnInputReceived?.Invoke(input);
        }
    }
}