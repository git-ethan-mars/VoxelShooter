using System.Collections;
using Mirror;

namespace Networking
{
    public abstract class MessageSplitter<T> where T : NetworkMessage
    {
        public abstract T[] SplitBytesIntoMessages(byte[] bytes, int maxPacketSize);

        public abstract IEnumerator SendMessages(T[] messages, NetworkConnectionToClient destination, float delayInSeconds);
    }
}