using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Carousel
{
    [Serializable]
    public class CarouselControl
    {
        public Button IncreaseButton => increaseButton;

        [SerializeField]
        private Button increaseButton;

        public Button DecreaseButton => decreaseButton;

        [SerializeField]
        private Button decreaseButton;

        public TextMeshProUGUI DisplayedValue => displayedValue;

        [SerializeField]
        private TextMeshProUGUI displayedValue;
    }
}