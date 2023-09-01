using Mirror;

namespace Networking
{
    public abstract class MessageHandler<TMessage> where TMessage : struct, NetworkMessage
    {
        protected abstract void OnMessageReceived(TMessage message);

        public void Register()
        {
            NetworkClient.RegisterHandler<TMessage>(OnMessageReceived);
        }

        public void Unregister()
        {
            NetworkClient.UnregisterHandler<TMessage>();
        }
    }
}