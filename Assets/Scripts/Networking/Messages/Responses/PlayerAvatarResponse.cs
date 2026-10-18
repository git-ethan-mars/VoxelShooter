using UnityEngine;

namespace Networking.Messages
{
	// Avatars are sent separately from the scoreboard so score updates stay small.
	public struct PlayerAvatarResponse : IResponse
	{
		public readonly int PlayerId;
		public readonly Texture2D Avatar;

		public PlayerAvatarResponse(int playerId, Texture2D avatar)
		{
			PlayerId = playerId;
			Avatar = avatar;
		}
	}
}
