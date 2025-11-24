using System.Linq;
namespace GamePlay
{
	public class BlockLootBox : LootBox
	{
		private const int BlockBonus = 50;

		protected override void OnPickUp(Character character)
		{
			Block block = character.Inventory.GetItem<Block>();

			if (block != null)
			{
				block.Amount.Value += BlockBonus;
			}
		}
	}
}