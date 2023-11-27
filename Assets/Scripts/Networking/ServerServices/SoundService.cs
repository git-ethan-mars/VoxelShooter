using Mirror;
using Networking.Messages.Responses;
using UnityEngine;

namespace Networking.ServerServices
{
    public class SoundService
    {
        public void SendSound(int soundId, Vector3 position)
        {
            NetworkServer.SendToReady(new SoundResponse(soundId, position));
        }
        
        public void StartContinuousSound(int soundId, NetworkIdentity source)
        {
            NetworkServer.SendToReady(new StartContinuousSoundResponse(soundId, source));
        }

        public void StopContinuousSound(int soundId, NetworkIdentity source)
        {
            NetworkServer.SendToReady(new StopContinuousSoundResponse(soundId, source));
        }
    }
}