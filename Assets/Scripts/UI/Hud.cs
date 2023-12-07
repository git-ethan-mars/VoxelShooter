using System.Collections.Generic;
using Infrastructure.Services.Input;
using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Hud : MonoBehaviour
    {
        public List<Image> Slots => slots;

        [SerializeField]
        private List<Image> slots;

        public List<GameObject> Boarders => boarders;

        [SerializeField]
        private List<GameObject> boarders;

        [SerializeField]
        private CanvasGroup canvasGroup;

        public Palette.Palette palette;
        public GameObject ammoInfo;
        public Image ammoType;
        public TextMeshProUGUI ammoCount;
        public GameObject itemInfo;
        public Image itemIcon;
        public TextMeshProUGUI itemCount;
        public HealthCounter healthCounter;
        private IInputService _inputService;
        
    }
}