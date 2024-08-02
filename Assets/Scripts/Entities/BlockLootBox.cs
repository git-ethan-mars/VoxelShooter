using Data;
using Mirror;
using Networking.Messages.Responses;

namespace Entities
{
    public class BlockLootBox : LootBox
    {
        private const int BlockBonus = 50;
        protected override void OnPickUp(NetworkConnectionToClient receiver)
        {
            var result = Server.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            playerData.BlockCount += BlockBonus;
            receiver.Send(new BlockUseResponse(playerData.BlockCount));
        }
    }
}