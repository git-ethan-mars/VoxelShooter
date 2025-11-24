using System;
using Mirror;
using R3;
namespace Networking.Core
{
	public class SyncReactiveProperty<T> : SyncObject
	{
		public T Value
		{
			get => _reactivePropertyWrapper.Value;
			set
			{
				if (!IsWritable())
				{
					throw new InvalidOperationException("SyncReactiveProperty can only be modified by the owner.");
				}

				_reactivePropertyWrapper.SetValueWithoutNotify(value);
				OnDirty?.Invoke();
			}
		}

		private readonly ReactivePropertyWrapper _reactivePropertyWrapper;

		public SyncReactiveProperty()
		{
			_reactivePropertyWrapper = new ReactivePropertyWrapper(OnInnerReactivePropertyChanged);
		}

		public override void OnSerializeAll(NetworkWriter writer)
		{
			writer.Write(_reactivePropertyWrapper.Value);
		}

		public override void OnSerializeDelta(NetworkWriter writer)
		{
			OnSerializeAll(writer);
		}

		public override void OnDeserializeAll(NetworkReader reader)
		{
			_reactivePropertyWrapper.SetValueWithoutNotify(reader.Read<T>());
		}

		public override void OnDeserializeDelta(NetworkReader reader)
		{
			OnDeserializeAll(reader);
		}

		public override void Reset()
		{
			_reactivePropertyWrapper.Value = default;
		}

		public override void ClearChanges()
		{
		}

		public static implicit operator ReactiveProperty<T>(SyncReactiveProperty<T> source)
		{
			return source._reactivePropertyWrapper;
		}

		private void OnInnerReactivePropertyChanged(T value)
		{
			Value = value;
		}

		private class ReactivePropertyWrapper : ReactiveProperty<T>
		{
			private readonly Action<T> _onValueChanged;

			public ReactivePropertyWrapper(Action<T> onValueChanged)
			{
				_onValueChanged = onValueChanged;
			}

			public override T Value
			{
				get => base.Value;
				set
				{
					base.Value = value;
					_onValueChanged?.Invoke(value);
				}
			}

			public void SetValueWithoutNotify(T value)
			{
				base.Value = value;
			}
		}
	}
}