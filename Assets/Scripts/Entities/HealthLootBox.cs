using Mirror;

namespace Entities
{
    public class HealthLootBox : LootBox
    {
        private const int HealBonus = 50;

        protected override void OnPickUp(NetworkConnectionToClient receiver)
        {
            Server.Heal(receiver, HealBonus);
        }
    }
}