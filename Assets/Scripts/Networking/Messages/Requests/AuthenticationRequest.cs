using UnityEngine;
namespace Networking.Messages
{
	public struct AuthenticationRequest : IRequest
	{
		public readonly string NickName;
		public readonly Texture2D Avatar;

		public AuthenticationRequest(string nickName, Texture2D avatar)
		{
			NickName = nickName;
			Avatar = avatar;
		}
	}
}