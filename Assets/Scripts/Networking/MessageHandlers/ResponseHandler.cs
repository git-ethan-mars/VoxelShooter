using Mirror;

namespace Networking.MessageHandlers
{
    public abstract class ResponseHandler<TResponse> where TResponse : struct, NetworkMessage
    {
        public void Register()
        {
            NetworkClient.RegisterHandler<TResponse>(OnResponseReceived);
        }

        public void Unregister()
        {
            NetworkClient.UnregisterHandler<TResponse>();
        }

        protected abstract void OnResponseReceived(TResponse response);
    }
}