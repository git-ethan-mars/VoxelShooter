namespace GamePlay
{
	public sealed class Spade : MeleeWeapon
	{
		private void Update()
		{
			if (InputService.IsFirstActionButtonDown())
			{
				Hit(RayCaster.CentredRay, false);
			}

			if (InputService.IsSecondActionButtonDown())
			{
				Hit(RayCaster.CentredRay, true);
			}
		}
	}
}