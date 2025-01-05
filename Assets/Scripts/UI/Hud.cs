using TMPro;
using UI.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Hud : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        //[SerializeField] private MiniMap miniMap;
        [SerializeField] private GameObject ammoInfo;
        [SerializeField] private Image ammoType;
        [SerializeField] private TextMeshProUGUI ammoCount;
        [SerializeField] private GameObject itemInfo;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemCount;
        public HealthCounter HealthCounter => healthCounter;
        [SerializeField] private HealthCounter healthCounter;
        public InventoryPresenter InventoryPresenter => inventoryPresenter;
        [SerializeField] private InventoryPresenter inventoryPresenter;
        public PalettePresenter PalettePresenter => palettePresenter;
        [SerializeField] private PalettePresenter palettePresenter;
        [SerializeField] private Image crosshairImage;
        [SerializeField] private Image scopeImage;
        
        public void ShowItemInfo(Sprite icon, string text)
        {
            itemInfo.SetActive(true);
            itemIcon.sprite = icon;
            itemCount.SetText(text);
        }

        public void HideItemInfo()
        {
            itemInfo.SetActive(false);
        }

        public void SetItemCount(string text)
        {
            itemCount.SetText(text);
        }

        public void ShowAmmoInfo(Sprite icon, string text)
        {
            ammoInfo.SetActive(true);
            ammoType.sprite = icon;
            ammoCount.SetText(text);
        }

        public void HideAmmoInfo()
        {
            ammoInfo.SetActive(false);
        }

        public void SetAmmoCount(string text)
        {
            ammoCount.SetText(text);
        }
    }
}