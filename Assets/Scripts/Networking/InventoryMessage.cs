using Mirror;

namespace Networking
{
    public struct InventoryMessage : NetworkMessage
    {
        public int[] Inventory;
    }
}