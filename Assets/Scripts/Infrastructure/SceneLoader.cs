using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Infrastructure
{
	public class SceneLoader
	{
		public async UniTask LoadAsync(string name)
		{
			if (SceneManager.GetActiveScene().name != name)
			{
				await SceneManager.LoadSceneAsync(name);	
			}
		}
	}
}