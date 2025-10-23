using System;
using Mirror;
using R3;
namespace Networking.Core
{
	public class SyncReactiveProperty<T> : SyncObject
	{
		public T Value
		{
			get => _reactiveProperty.Value;
			set
			{
				if (!IsWritable())
				{
					throw new InvalidOperationException("SyncReactiveProperty can only be modified by the owner.");	
				}
				_reactiveProperty.Value = value;
				OnDirty?.Invoke();
			}
		}

		private readonly ReactiveProperty<T> _reactiveProperty = new ReactiveProperty<T>();
			
		public override void OnSerializeAll(NetworkWriter writer)
		{
			writer.Write(_reactiveProperty.Value);
		}

		public override void OnSerializeDelta(NetworkWriter writer)
		{
			OnSerializeAll(writer);
		}

		public override void OnDeserializeAll(NetworkReader reader)	
		{
			_reactiveProperty.Value = reader.Read<T>();
		}

		public override void OnDeserializeDelta(NetworkReader reader)
		{
			OnDeserializeAll(reader);
		}

		public override void Reset()
		{
			_reactiveProperty.Value = default;
		}

		public override void ClearChanges()
		{
		}
		
		public static implicit operator ReactiveProperty<T>(SyncReactiveProperty<T> source)
		{
			return source._reactiveProperty;
		}
	}
}