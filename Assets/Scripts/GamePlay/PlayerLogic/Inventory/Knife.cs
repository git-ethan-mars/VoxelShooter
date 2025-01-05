namespace GamePlay
{
	public sealed class Knife : MeleeWeapon
	{
		private void Update()
		{
			if (InputService.IsFirstActionButtonDown())
			{
				Hit(RayCaster.CentredRay, false);
			}
		}
	}
}