using Mirror;
using R3;
using UnityEngine;
namespace Networking.Core
{
	public class SyncReactiveProperty<T> : SyncObject
	{
		public T Value
		{
			get => _reactiveProperty.Value;
			set => _reactiveProperty.Value = value;
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