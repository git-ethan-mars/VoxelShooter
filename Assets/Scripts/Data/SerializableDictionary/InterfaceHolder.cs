using UnityEngine;

namespace Data.SerializableDictionary
{
	[System.Serializable]
	public class InterfaceHolder<T> where T : class
	{
		[SerializeField] private MonoBehaviour value;

		public T Value
		{
			get
			{
				if (value == null)
				{
					Debug.LogError("value is null");
					return null;
				}

				var castValue = value as T;
				if (castValue == null)
				{
					Debug.LogError($"value cannot be cast to {typeof(T)}. It is of type {value.GetType()}");
				}

				return castValue;
			}
		}

		public InterfaceHolder(MonoBehaviour value)
		{
			this.value = value;
		}
	}
}
