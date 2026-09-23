using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public abstract class ListView<T> : MonoBehaviour where T : Component
	{
		protected readonly List<T> Items = new List<T>();

		[SerializeField] private T itemPrefab;

		[SerializeField] private Transform container;

		private readonly LinkedList<T> _freeList = new LinkedList<T>();

		public T SpawnElement()
		{
			T item;
			if (_freeList.First != null)
			{
				item = _freeList.First.Value;
				item.gameObject.SetActive(true);
				_freeList.RemoveFirst();
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
			for (int i = Items.Count - 1; i >= 0; i--)
			{
				T item = Items[i];
				item.gameObject.SetActive(false);
				_freeList.AddFirst(item);
			}

			Items.Clear();
		}

		protected void DespawnElement(T item)
		{
			if (item != null && Items.Remove(item))
			{
				item.gameObject.SetActive(false);
				_freeList.AddLast(item);
			}
		}
	}
}
