using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
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

        public CanvasGroup CanvasGroup => canvasGroup;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private MiniMap miniMap;

        public MiniMap Minimap => miniMap;

        [SerializeField]
        private Palette.Palette palette;

        public Palette.Palette Palette => palette;


        [SerializeField]
        private GameObject ammoInfo;

        public GameObject AmmoInfo => ammoInfo;


        [SerializeField]
        private Image ammoType;

        public Image AmmoType => ammoType;


        [SerializeField]
        private TextMeshProUGUI ammoCount;

        public TextMeshProUGUI AmmoCount => ammoCount;


        [SerializeField]
        private GameObject itemInfo;

        public GameObject ItemInfo => itemInfo;


        [SerializeField]
        private Image itemIcon;

        public Image ItemIcon => itemIcon;


        [SerializeField]
        private TextMeshProUGUI itemCount;

        public TextMeshProUGUI ItemCount => itemCount;


        [SerializeField]
        private HealthCounter healthCounter;

        public HealthCounter HealthCounter => healthCounter;
    }
}