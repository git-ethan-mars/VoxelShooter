using Mirror;
using UnityEngine;
namespace Networking.Messages
{
	public struct SurroundingSoundResponse : NetworkMessage
	{
		public readonly int SoundId;
		public readonly Vector3 Position;

		public SurroundingSoundResponse(int soundId, Vector3 position)
		{
			SoundId = soundId;
			Position = position;
		}
	}
}