using Reflex.Core;
using Reflex.Injectors;
using UnityEngine;
namespace Infrastructure
{
	public class GameRunner : MonoBehaviour
	{
		public GameBootstrapper bootstrapperPrefab;

		private void Awake()
		{
			var bootstrapper = FindAnyObjectByType<GameBootstrapper>();
			
			if (!bootstrapper)
			{
				bootstrapper = Instantiate(bootstrapperPrefab);
				GameObjectInjector.InjectSingle(bootstrapper.gameObject, Container.ProjectContainer);
			}
		}
	}
}