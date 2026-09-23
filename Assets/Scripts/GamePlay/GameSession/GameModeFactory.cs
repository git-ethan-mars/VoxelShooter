using Cysharp.Threading.Tasks;
using Data;
using Reflex.Extensions;
using Reflex.Injectors;
using UnityEngine.SceneManagement;
namespace GamePlay
{
	public class GameModeFactory
	{
		public async UniTask<GameMode> CreateGameMode(GameSettings gameSettings)
		{
			var deathMatch = (DeathMatch)ConstructorInjector.Construct(typeof(DeathMatch),
				SceneManager.GetActiveScene().GetSceneContainer());
			await deathMatch.Start(gameSettings);
			return deathMatch;
		}
	}
}