using System;
using System.Globalization;
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

        public float SliderValue { get; private set; }

        public void Construct(int initializedValue, int minValue, int maxValue)
        {
            slider.wholeNumbers = true;
            slider.onValueChanged.AddListener(UpdateDisplayedValue);
            slider.maxValue = maxValue;
            slider.minValue = minValue;
            slider.value = initializedValue;
        }

        public void Construct(float initializedValue, float minValue, float maxValue)
        {
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
                SliderValue = (float) Math.Round(value, 1);
            }
            else
            {
                SliderValue = value;
            }

            displayedValue.SetText(SliderValue.ToString(CultureInfo.InvariantCulture));
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(UpdateDisplayedValue);
        }
    }
}