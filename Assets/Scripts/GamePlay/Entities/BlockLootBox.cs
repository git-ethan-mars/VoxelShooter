using Entities;
using Entities.PlayerLogic;
using Inventory.Block;

namespace GamePlay.Entities
{
    public class BlockLootBox : LootBox
    {
        private const int BlockBonus = 50;
        protected override void OnPickUp(Character character)
        {
            character.Inventory.ApplyEffectToItems<Block>(block => block.Data.Amount += BlockBonus);
        }
    }
}