using System;
using Mirror;
using Networking.Messages.Requests;
using Random = UnityEngine.Random;

namespace Networking
{
    public class LocalAuthenticator : NetworkAuthenticator
    {
        public override void OnClientAuthenticate()
        {
            var id = (ulong)Random.Range(0, int.MaxValue);
            var nickName = Guid.NewGuid().ToString();
            NetworkClient.Send(new AuthenticationRequest(id, nickName));
        }
    }
}