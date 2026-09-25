using GamePlay;
using R3;
using Reflex.Attributes;
using TMPro;
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
		[SerializeField] private HealthCounter healthCounter;
		[SerializeField] private PaletteView paletteView;
		[SerializeField] private BlueprintMenuView blueprintMenu;
		[SerializeField] private Image scopeImage;
		[SerializeField] private Image crosshairImage;

		private CharacterProvider _characterProvider;

		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

		[Inject]
		private void Construct(CharacterProvider characterProvider)
		{
			_characterProvider = characterProvider;
		}

		private void OnEnable()
		{
			_characterProvider.Character
				.Where(character => character != null)
				.SelectMany(character => character.HealthSystem.Health)
				.Subscribe(health => healthCounter.SetHealthValue(health.ToString()))
				.AddTo(this);
		}

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

		public void SetCrosshairIcon(Sprite crosshairIcon)
		{
			crosshairImage.sprite = crosshairIcon;
		}

		public void SetCrosshairVisibility(bool enable)
		{
			crosshairImage.gameObject.SetActive(enable);
		}

		public void SetScopeIcon(Sprite scopeIcon)
		{
			if (scopeIcon != null)
			{
				scopeImage.gameObject.SetActive(true);
				scopeImage.sprite = scopeIcon;
			}
			else
			{
				scopeImage.gameObject.SetActive(false);
				scopeImage.sprite = null;
			}
		}

		public void ShowPalette()
		{
			paletteView.Show();
		}

		public void HidePalette()
		{
			paletteView.Hide();
		}

		public void ShowBlueprintMenu(Blueprint blueprint)
		{
			blueprintMenu.Bind(blueprint);
		}

		public void HideBlueprintMenu()
		{
			blueprintMenu.Unbind();
		}
	}
}
