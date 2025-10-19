using UnityEngine;
namespace Services
{
	public interface IAssetProvider
	{
		GameObject Instantiate(string path);
		GameObject Instantiate(string path, Transform parent);
		GameObject Instantiate(string path, Vector3 position, Quaternion rotation);

		GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent);
		T Load<T>(string path) where T : Object;
		T[] LoadAll<T>(string path) where T : Object;
		GameObject Instantiate(GameObject prefab, Transform parent = null);
	}
}