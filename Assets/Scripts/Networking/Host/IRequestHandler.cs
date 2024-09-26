using Mirror;
using Networking.Messages.Requests;

namespace Networking.Host
{
    public interface IRequestHandler<TRequest> where TRequest : struct, IMirrorRequest 
    {
        void OnRequestReceived(NetworkConnectionToClient connection, TRequest request);
    }
}