using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Hud : MonoBehaviour
    {
        public GameObject inventory;
        public GameObject palette;
        public GameObject ammoInfo;
        public Image ammoType;
        public TextMeshProUGUI ammoCount;
        public GameObject itemInfo;
        public Image itemIcon;
        public TextMeshProUGUI itemCount;
        public HealthCounter healthCounter;
    }
}
