using System.Collections.Generic;
using UnityEngine;
namespace UI
{
	public abstract class ListView<T> : MonoBehaviour where T : Component
	{
		[SerializeField]
		private T itemPrefab;

		[SerializeField]
		private Transform container;
		private readonly Queue<T> _freeList = new Queue<T>();

		protected readonly List<T> Items = new List<T>();

		public T SpawnElement()
		{
			if (_freeList.TryDequeue(out T item))
			{
				item.gameObject.SetActive(true);
			}
			else
			{
				item = Instantiate(itemPrefab, container);
			}

			Items.Add(item);
			return item;
		}

		public void Clear()
		{
			for (int i = 0, count = Items.Count; i < count; i++)
			{
				T item = Items[i];
				item.gameObject.SetActive(false);
				_freeList.Enqueue(item);
			}

			Items.Clear();
		}

		protected void DespawnElement(T item)
		{
			if (item != null && Items.Remove(item))
			{
				item.gameObject.SetActive(false);
				_freeList.Enqueue(item);
			}
		}

	}
}