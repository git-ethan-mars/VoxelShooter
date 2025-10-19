namespace GamePlay
{
	public class HealthLootBox : LootBox
	{
		private const int HealBonus = 50;

		protected override void OnPickUp(Character character)
		{
			character.Heal(HealBonus);
		}
	}
}