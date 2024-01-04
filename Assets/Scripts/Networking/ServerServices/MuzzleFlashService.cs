using Data;
using Mirror;
using Networking.Messages.Responses;

namespace Networking.ServerServices
{
    public class MuzzleFlashService
    {
        public MuzzleFlashService()
        {
            
        }

        public void StartMuzzleFlash(NetworkIdentity source)
        {
            NetworkServer.SendToReady(new StartMuzzleFlashResponse(source));
        }
        
        public void StopMuzzleFlash(NetworkIdentity source)
        {
            NetworkServer.SendToReady(new StopMuzzleFlashResponse(source));
        }
    }
}