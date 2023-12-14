using Data;
using Infrastructure.Services.StaticData;
using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class AudioService
    {
        private readonly IStaticDataService _staticData;

        public AudioService(IStaticDataService staticData)
        {
            _staticData = staticData;
        }

        public void SendAudio(AudioData audio, Vector3 position)
        {
            NetworkServer.SendToReady(new SurroundingSoundResponse(_staticData.GetAudioIndex(audio), position));
        }
        public void SendAudio(AudioData audio, NetworkIdentity source)
        {
            NetworkServer.SendToReady(new PlayerSoundResponse(_staticData.GetAudioIndex(audio), source));
        }

        public void StartContinuousAudio(AudioData audio, NetworkIdentity source)
        {
            NetworkServer.SendToReady(new StartContinuousSoundResponse(_staticData.GetAudioIndex(audio), source));
        }

        public void StopContinuousSound(NetworkIdentity source)
        {
            NetworkServer.SendToReady(new StopContinuousSoundResponse(source));
        }
    }
}