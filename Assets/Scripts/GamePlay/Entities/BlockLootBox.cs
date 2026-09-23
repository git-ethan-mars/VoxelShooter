namespace GamePlay
{
	public class BlockLootBox : LootBox
	{
		private const int BlockBonus = 50;

		protected override void OnPickUp(Character character)
		{
			character.Inventory.VoxelAmount.Value += BlockBonus;
		}
	}
}
