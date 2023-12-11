using System;
using System.Globalization;
using Infrastructure;
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

        public ObservableVariable<float> SliderValue { get; private set; }

        public void Construct(int initializedValue, int minValue, int maxValue)
        {
            SliderValue = new ObservableVariable<float>(initializedValue);
            slider.wholeNumbers = true;
            slider.onValueChanged.AddListener(UpdateDisplayedValue);
            slider.maxValue = maxValue;
            slider.minValue = minValue;
            slider.value = initializedValue;
        }

        public void Construct(float initializedValue, float minValue, float maxValue)
        {
            SliderValue = new ObservableVariable<float>(initializedValue);
            slider.wholeNumbers = false;
            slider.onValueChanged.AddListener(UpdateDisplayedValue);
            slider.maxValue = maxValue;
            slider.minValue = minValue;
            slider.value = initializedValue;
        }

        private void UpdateDisplayedValue(float value)
        {
            if (!slider.wholeNumbers)
            {
                SliderValue.Value = (float) Math.Round(value, 1);
            }
            else
            {
                SliderValue.Value = value;
            }

            displayedValue.SetText(SliderValue.Value.ToString(CultureInfo.InvariantCulture));
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(UpdateDisplayedValue);
        }
    }
}