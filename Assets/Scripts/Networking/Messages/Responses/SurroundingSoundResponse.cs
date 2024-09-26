using UnityEngine;

namespace Networking.Messages.Responses
{
	public struct SurroundingSoundResponse : IMirrorResponse
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