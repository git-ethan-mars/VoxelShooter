namespace Networking.Messages.Requests
{
    public struct AuthenticationRequest : IMirrorRequest
    {
        public readonly ulong Id;
        public readonly string NickName;

        public AuthenticationRequest(ulong id, string nickName)
        {
            Id = id;
            NickName = nickName;
        }
    }
}