using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class GameClassInfo : MonoBehaviour
	{
		[field: SerializeField] public Button ChooseClassButton { get; private set; }
		
		[SerializeField] private TextMeshProUGUI classNameText;
		[SerializeField] private TextMeshProUGUI healthValueText;
		[SerializeField] private Image mainWeaponIcon;
		[SerializeField] private Image mainWeaponProjectileIcon;
		[SerializeField] private TextMeshProUGUI mainWeaponDescription;
		[SerializeField] private Image secondaryWeaponIcon;
		[SerializeField] private Image secondaryWeaponProjectileIcon;
		[SerializeField] private TextMeshProUGUI secondaryWeaponDescription;
		[SerializeField] private Image meleeWeaponIcon;

		public void SetHealthValue(int health)
		{
			healthValueText.SetText(health.ToString());
		}

		public void SetClassName(GameClass gameClass)
		{
			classNameText.SetText(gameClass.ToString());
		}

		public void SetMainWeapon(Sprite weaponIcon, Sprite projectileIcon, string description, bool isMain)
		{
			if (isMain)
			{
				mainWeaponIcon.sprite = weaponIcon;
				mainWeaponProjectileIcon.sprite = projectileIcon;
				mainWeaponDescription.SetText(description);
			}
			else
			{
				secondaryWeaponIcon.sprite = weaponIcon;
				secondaryWeaponProjectileIcon.sprite = projectileIcon;
				secondaryWeaponDescription.SetText(description);
			}
		}

		public void SetMeleeWeaponIcon(Sprite weaponIcon)
		{
			meleeWeaponIcon.sprite = weaponIcon;
		}
	}
}