using System.Collections.Generic;
using R3;
using UnityEngine;
namespace UI
{
	public sealed class Limitation : ReactiveProperty<int>
	{
		private readonly int _maxValue;
		private readonly int _minValue;

		public Limitation(int minValue, int maxValue) : base(minValue, EqualityComparer<int>.Default, false)
		{
			_minValue = minValue;
			_maxValue = maxValue;

			OnValueChanging(ref GetValueRef());
		}

		protected override void OnValueChanging(ref int value)
		{
			base.OnValueChanging(ref value);
			value = Mathf.Clamp(value, _minValue, _maxValue);
		}

		public void Reset()
		{
			Value = _minValue;
		}
	}
}