using System;
using System.Globalization;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class SliderWithDisplayedValue : MonoBehaviour
	{
		[SerializeField]
		private TextMeshProUGUI displayedValue;
		[SerializeField]
		private Slider slider;

		public void Construct(int initializedValue, int minValue, int maxValue)
		{
			slider.wholeNumbers = true;
			slider.maxValue = maxValue;
			slider.minValue = minValue;
			slider.value = initializedValue;
			Slider = slider.OnValueChangedAsObservable().ToReadOnlyReactiveProperty().AddTo(this);
			Slider.Subscribe(value =>
			{
				displayedValue.SetText(value.ToString(CultureInfo.InvariantCulture));
			}).AddTo(this);
		}

		public void Construct(float initializedValue, float minValue, float maxValue)
		{
			slider.wholeNumbers = false;
			slider.maxValue = maxValue;
			slider.minValue = minValue;
			slider.value = initializedValue;
			Slider = slider.OnValueChangedAsObservable().ToReadOnlyReactiveProperty().AddTo(this);
			Slider
				.Select(value => (float)Math.Round(value, 1))
				.Subscribe(value =>
				{
					displayedValue.SetText(value.ToString(CultureInfo.InvariantCulture));
				}).AddTo(this);
		}

		public ReadOnlyReactiveProperty<float> Slider { get; private set; }
	}
}