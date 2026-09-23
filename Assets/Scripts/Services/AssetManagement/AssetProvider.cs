using UnityEngine;

namespace Services
{
	public class AssetProvider : IAssetProvider
	{
		public T Load<T>(string path) where T : Object
		{
			T obj = Resources.Load<T>(path);
			return obj;
		}

		public T[] LoadAll<T>(string path) where T : Object
		{
			T[] objects = Resources.LoadAll<T>(path);
			return objects;
		}

		public GameObject Instantiate(GameObject prefab, Transform parent = null)
		{
			return Object.Instantiate(prefab, parent);
		}

		public GameObject Instantiate(string path)
		{
			GameObject prefab = Resources.Load<GameObject>(path);
			return Object.Instantiate(prefab);
		}

		public GameObject Instantiate(string path, Transform transform)
		{
			GameObject prefab = Resources.Load<GameObject>(path);
			return Object.Instantiate(prefab, transform);
		}

		public GameObject Instantiate(string path, Vector3 position, Quaternion rotation)
		{
			GameObject prefab = Resources.Load<GameObject>(path);
			return Object.Instantiate(prefab, position, rotation);
		}

		public GameObject Instantiate(string path, Vector3 position, Quaternion rotation, Transform parent)
		{
			GameObject prefab = Resources.Load<GameObject>(path);
			return Object.Instantiate(prefab, position, rotation, parent);
		}
	}
}
