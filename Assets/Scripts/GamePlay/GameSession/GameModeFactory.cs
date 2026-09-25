using Cysharp.Threading.Tasks;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine.SceneManagement;

namespace GamePlay
{
	public class GameModeFactory
	{
		public GameMode CreateGameMode()
		{
			var deathMatch = (DeathMatch)ConstructorInjector.Construct(typeof(DeathMatch),
				SceneManager.GetActiveScene().GetSceneContainer());
			return deathMatch;
		}
	}
}
