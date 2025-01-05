using Networking.Messages.Responses;

namespace Networking.Client
{
    public partial class MirrorClient : IResponseHandler<AuthenticationResponse>
    {
        public void OnResponseReceived(AuthenticationResponse response)
        {
            _networkManager.authenticator.OnClientAuthenticated.Invoke();
        }
    }
}