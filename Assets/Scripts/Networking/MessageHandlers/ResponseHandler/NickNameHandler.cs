using Networking.Messages.Responses;
using PlayerLogic;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class NickNameHandler : ResponseHandler<NickNameResponse>
    {
        protected override void OnResponseReceived(NickNameResponse response)
        {
            if (response.Identity == null)
            {
                return;
            }

            if (response.Identity.TryGetComponent<Player>(out var player))
            {
                player.NickName = response.NickName;
            }
        }
    }
}