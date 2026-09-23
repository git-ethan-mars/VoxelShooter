using Mirror;
namespace Networking.Messages
{
	public struct StartContinuousSoundResponse : NetworkMessage
	{
		public readonly int SoundId;
		public readonly NetworkIdentity Source;

		public StartContinuousSoundResponse(int soundId, NetworkIdentity source)
		{
			SoundId = soundId;
			Source = source;
		}
	}
}