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
				{
					throw new InvalidOperationException("SyncReactiveDictionary can only be modified by the owner.");
				}

				_wrapper.SetItemWithoutNotify(key, value);
				OnDirty?.Invoke();
			}
		}

		public SyncReactiveDictionary()
		{
			_wrapper = new ReactiveDictionaryWrapper(OnInnerChanged);
		}

		public override void Reset()
		{
			_wrapper.ClearWithoutNotify();
			_wrapper.NotifyReset();
		}

		public bool TryGetValue(TKey key, out TValue value) => _wrapper.TryGetValue(key, out value);
		public bool ContainsKey(TKey key) => _wrapper.ContainsKey(key);

		public void Add(TKey key, TValue value)
		{
			if (!IsWritable())
			{
				throw new InvalidOperationException("SyncReactiveDictionary can only be modified by the owner.");
			}

			_wrapper.AddWithoutNotify(key, value);
			OnDirty?.Invoke();
		}

		public bool Remove(TKey key)
		{
			if (!IsWritable())
			{
				throw new InvalidOperationException("SyncReactiveDictionary can only be modified by the owner.");
			}

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
			{
				throw new InvalidOperationException("SyncReactiveDictionary can only be modified by the owner.");
			}

			_wrapper.ClearWithoutNotify();
			OnDirty?.Invoke();
		}

		// --- Serialization ---

		public override void OnSerializeAll(NetworkWriter writer)
		{
			writer.WriteInt(_wrapper.Count);
			foreach (KeyValuePair<TKey, TValue> kv in _wrapper)
			{
				writer.Write(kv.Key);
				writer.Write(kv.Value);
			}
		}

		public override void OnSerializeDelta(NetworkWriter writer)
		{
			// Full serialization for simplicity. If needed, keep a set of
			// "dirty" keys and send only them plus the list of removed ones.
			OnSerializeAll(writer);
		}

		public override void OnDeserializeAll(NetworkReader reader)
		{
			int count = reader.ReadInt();

			_wrapper.ClearWithoutNotify();

			for (int i = 0; i < count; i++)
			{
				TKey key = reader.Read<TKey>();
				TValue value = reader.Read<TValue>();
				_wrapper.SetItemWithoutNotify(key, value);
			}

			_wrapper.NotifyReset();
		}

		public override void OnDeserializeDelta(NetworkReader reader)
		{
			OnDeserializeAll(reader);
		}

		public override void ClearChanges()
		{
		}

		// Implicit conversion so the UI can work with ReactiveDictionary directly
		public static implicit operator ObservableDictionary<TKey, TValue>(SyncReactiveDictionary<TKey, TValue> source)
		{
			return source._wrapper;
		}

		private void OnInnerChanged()
		{
			// The owner changed the dictionary through ReactiveDictionary, so mark the SyncObject dirty.
			// Instead of assigning Value directly (as for a single property), just invoke OnDirty.
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
				// Bypass OnNext without notifying subscribers manually
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

			// Explicit signal to subscribers after a bulk update (deserialization/Reset)
			public void NotifyReset()
			{
				// ReactiveDictionary in R3 raises its own events
				// (Add/Remove/Replace/Clear). If a single "Reset" is needed, use
				// something like ForceNotifyAll or recreate the subscriptions on the UI side.
			}
		}
	}
}
