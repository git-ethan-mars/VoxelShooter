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
            var result = Server.Data.TryGetPlayerData(receiver, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            for (var i = 0; i < playerData.Items.Count; i++)
            {
                if (playerData.Items[i].itemType == ItemType.Block)
                {
                    var blockItemData = (BlockItemData) playerData.ItemData[i];
                    blockItemData.Amount += BlockBonus;
                    receiver.Send(new ItemUseResponse(i, blockItemData.Amount));
                }
            }
        }
    }
}