using Mirror;

namespace Networking.Messages.Responses
{
	public struct PlayerSoundResponse : IMirrorResponse
	{
		public readonly int SoundId;
		public readonly NetworkIdentity Source;

		public PlayerSoundResponse(int soundId, NetworkIdentity source)
		{
			SoundId = soundId;
			Source = source;
		}
	}
}