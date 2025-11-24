using GamePlay;
using R3;
using Reflex.Attributes;
using TMPro;
using UI.Inventory;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class Hud : MonoBehaviour
	{
		[SerializeField] private GameObject ammoInfo;
		[SerializeField] private Image ammoType;
		[SerializeField] private TextMeshProUGUI ammoCount;
		[SerializeField] private GameObject itemInfo;
		[SerializeField] private Image itemIcon;
		[SerializeField] private TextMeshProUGUI itemCount;
		
		private CharacterProvider _characterProvider;

		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
		[field: SerializeField] public HealthCounter HealthCounter { get; private set; }
		[field: SerializeField] public InventoryPresenter InventoryPresenter { get; private set; }
		[field: SerializeField] public PalettePresenter PalettePresenter { get; private set; }
		[field: SerializeField] public PaletteView PaletteView { get; private set; }
		[field: SerializeField] public MiniMap MiniMap { get; private set; }
		[field: SerializeField] public Image ScopeImage { get; private set; }
		[field: SerializeField] public Image CrosshairImage { get; private set; }


		[Inject]
		private void Construct(CharacterProvider characterProvider)
		{
			_characterProvider = characterProvider;
		}

		public void Initialize()
		{
			_characterProvider.Character
				.Where(character => character != null)
				.SelectMany(character => character.HealthSystem.Health)
				.Subscribe(health => HealthCounter.SetHealthValue(health.ToString()))
				.AddTo(this);
		}

		public void ShowItemInfo(Sprite icon, string text)
		{
			itemInfo.SetActive(true);
			itemIcon.sprite = icon;
			itemCount.SetText(text);
			CrosshairImage.gameObject.SetActive(true);
		}

		public void HideItemInfo()
		{
			itemInfo.SetActive(false);
			CrosshairImage.gameObject.SetActive(false);
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
			CrosshairImage.gameObject.SetActive(true);
		}

		public void HideAmmoInfo()
		{
			ammoInfo.SetActive(false);
			CrosshairImage.gameObject.SetActive(false);
		}

		public void SetAmmoCount(string text)
		{
			ammoCount.SetText(text);
		}

		public void SetCrosshairIcon(Sprite icon)
		{
			CrosshairImage.sprite = icon;
		}
		
		public void ShowPalette()
		{
			PalettePresenter.Initialize();
		}

		public void HidePalette()
		{
			PaletteView.Clear();
		}
	}
}