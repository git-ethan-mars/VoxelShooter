using Mirror;

namespace Networking.MessageHandlers
{
    public abstract class RequestHandler<TRequest> where TRequest : struct, NetworkMessage 
    {
        public void Register()
        {
            NetworkServer.RegisterHandler<TRequest>(OnRequestReceived);
        }

        public void Unregister()
        {
            NetworkServer.UnregisterHandler<TRequest>();
        }

        protected abstract void OnRequestReceived(NetworkConnectionToClient connection, TRequest request);
    }
}