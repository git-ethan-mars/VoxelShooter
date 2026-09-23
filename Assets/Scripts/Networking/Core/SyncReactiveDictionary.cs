using System;
using System.Collections.Generic;
using Mirror;
using ObservableCollections;

namespace Networking.Core
{
	public class SyncReactiveDictionary<TKey, TValue> : SyncObject
	{
		private readonly ReactiveDictionaryWrapper _wrapper;
		public IReadOnlyDictionary<TKey, TValue> Values => _wrapper;

		public int Count => _wrapper.Count;

		public TValue this[TKey key]
		{
			get => _wrapper[key];
			set
			{
				if (!IsWritable())
					throw new InvalidOperationException("SyncReactiveDictionary can only be modified by the owner.");

				_wrapper.SetItemWithoutNotify(key, value);
				OnDirty?.Invoke();
			}
		}

		public SyncReactiveDictionary()
		{
			_wrapper = new ReactiveDictionaryWrapper(OnInnerChanged);
		}

		public bool TryGetValue(TKey key, out TValue value) => _wrapper.TryGetValue(key, out value);
		public bool ContainsKey(TKey key) => _wrapper.ContainsKey(key);

		public void Add(TKey key, TValue value)
		{
			if (!IsWritable())
				throw new InvalidOperationException("SyncReactiveDictionary can only be modified by the owner.");

			_wrapper.AddWithoutNotify(key, value);
			OnDirty?.Invoke();
		}

		public bool Remove(TKey key)
		{
			if (!IsWritable())
				throw new InvalidOperationException("SyncReactiveDictionary can only be modified by the owner.");

			if (_wrapper.RemoveWithoutNotify(key))
			{
				OnDirty?.Invoke();
				return true;
			}

			return false;
		}

		public void Clear()
		{
			if (!IsWritable())
				throw new InvalidOperationException("SyncReactiveDictionary can only be modified by the owner.");

			_wrapper.ClearWithoutNotify();
			OnDirty?.Invoke();
		}

		// --- Сериализация ---

		public override void OnSerializeAll(NetworkWriter writer)
		{
			writer.WriteInt(_wrapper.Count);
			foreach (var kv in _wrapper)
			{
				writer.Write(kv.Key);
				writer.Write(kv.Value);
			}
		}

		public override void OnSerializeDelta(NetworkWriter writer)
		{
			// Для простоты — полная сериализация. При необходимости можно
			// хранить набор "грязных" ключей и слать только их + список удалённых.
			OnSerializeAll(writer);
		}

		public override void OnDeserializeAll(NetworkReader reader)
		{
			int count = reader.ReadInt();

			_wrapper.ClearWithoutNotify();

			for (int i = 0; i < count; i++)
			{
				var key = reader.Read<TKey>();
				var value = reader.Read<TValue>();
				_wrapper.SetItemWithoutNotify(key, value);
			}

			_wrapper.NotifyReset();
		}

		public override void OnDeserializeDelta(NetworkReader reader)
		{
			OnDeserializeAll(reader);
		}

		public override void Reset()
		{
			_wrapper.ClearWithoutNotify();
			_wrapper.NotifyReset();
		}

		public override void ClearChanges()
		{
		}

		// Неявное преобразование — чтобы UI работал с ReactiveDictionary напрямую
		public static implicit operator ObservableDictionary<TKey, TValue>(SyncReactiveDictionary<TKey, TValue> source)
		{
			return source._wrapper;
		}

		private void OnInnerChanged()
		{
			// Владелец изменил словарь через ReactiveDictionary — помечаем SyncObject грязным.
			// Здесь вместо прямого присвоения Value (как в одиночном свойстве) просто дергаем OnDirty.
			OnDirty?.Invoke();
		}

		// --- Wrapper ---

		private class ReactiveDictionaryWrapper : ObservableDictionary<TKey, TValue>
		{
			private readonly Action _onChanged;

			public ReactiveDictionaryWrapper(Action onChanged)
			{
				_onChanged = onChanged;
			}

			public void SetItemWithoutNotify(TKey key, TValue value)
			{
				// Обходим OnNext, но не уведомляем подписчиков вручную
				base[key] = value;
			}

			public void AddWithoutNotify(TKey key, TValue value)
			{
				base.Add(key, value);
			}

			public bool RemoveWithoutNotify(TKey key)
			{
				return base.Remove(key);
			}

			public void ClearWithoutNotify()
			{
				base.Clear();
			}

			// Явный сигнал подписчикам после массового обновления (десериализация/Reset)
			public void NotifyReset()
			{
				// ReactiveDictionary в R3 поддерживает OnNext через собственные события
				// (Add/Remove/Replace/Clear). Если нужен единый "Reset" — используйте
				// что-то вроде ForceNotifyAll, либо пересоздайте подписки на стороне UI.
			}
		}
	}
}
