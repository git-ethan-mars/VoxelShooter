using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Hud : MonoBehaviour
    {
        public GameObject palette;
        public InventoryView inventory;
        public GameObject ammoInfo;
        public Image ammoType;
        public GameObject ammoCount;
        public GameObject blockInfo;
        public TextMeshProUGUI blockCount;
        public HealthCounter healthCounter;
    }
}
