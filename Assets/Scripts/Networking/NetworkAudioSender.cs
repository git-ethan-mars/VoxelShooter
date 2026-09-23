using Mirror;
using Networking.Messages;
using UnityEngine;
using AudioType = Data.AudioType;

namespace Networking
{
	public class NetworkAudioSender
	{
		private readonly VSNetworkManager _networkManager;

		public NetworkAudioSender(VSNetworkManager networkManager)
		{
			_networkManager = networkManager;
		}

		public void SendAudio(AudioType audioType, Vector3 position)
		{
			var audioResponse = new StaticAudioResponse(audioType, position);
			_networkManager.SendResponseToAll(audioResponse, true);
		}

		public void SendAudio(AudioType audioType, NetworkIdentity target, bool isSpatial)
		{
			var audioResponse = new DynamicAudioResponse(audioType, target, isSpatial);
			_networkManager.SendResponseToAll(audioResponse, true);
		}
	}
}
