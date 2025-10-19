using R3;
namespace GamePlay
{
	public class CharacterProvider
	{
		public ReactiveProperty<Character> Character { get; private set; } = new ReactiveProperty<Character>();
	}
}