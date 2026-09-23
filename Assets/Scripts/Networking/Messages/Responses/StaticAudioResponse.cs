using UnityEngine;
using AudioType = Data.AudioType;
namespace Networking.Messages
{
	public struct StaticAudioResponse : IResponse
	{
		public readonly AudioType AudioType;
		public readonly Vector3 Position;
		
		public StaticAudioResponse(AudioType audioType, Vector3 position)
		{
			AudioType = audioType;
			Position = position;
		}
	}
}