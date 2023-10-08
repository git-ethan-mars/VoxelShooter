using System.Collections;
using Mirror;

namespace Networking
{
    public abstract class MessageSplitter<TMessage, T1> where TMessage : NetworkMessage
    {
        public abstract TMessage[] SplitBytesIntoMessages(T1[] input1, int maxPacketSize);

        public abstract IEnumerator SendMessages(TMessage[] messages, NetworkConnectionToClient destination,
            float delayInSeconds);
    }

    public abstract class MessageSplitter<TMessage, T1, T2> where TMessage : NetworkMessage
    {
        public abstract TMessage[] SplitBytesIntoMessages(T1[] input1, T2[] input2, int maxPacketSize);

        public abstract IEnumerator SendMessages(TMessage[] messages, NetworkConnectionToClient destination,
            float delayInSeconds);
    }
}