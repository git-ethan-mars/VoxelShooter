using Data;
using Mirror;
namespace Networking.Messages
{
	public struct DynamicAudioResponse : IResponse
	{
		public readonly AudioType AudioType;
		public readonly NetworkIdentity NetworkIdentity;
		public readonly bool IsSpatial;

		public DynamicAudioResponse(AudioType audioType, NetworkIdentity networkIdentity, bool isSpatial)
		{
			AudioType = audioType;
			NetworkIdentity = networkIdentity;
			IsSpatial = isSpatial;
		}
	}
}