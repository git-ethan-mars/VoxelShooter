using System.Collections.Generic;
using Data;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ChooseClassMenu : MonoBehaviour
	{
		[SerializeField] private Button exitButton;
		[SerializeField] private List<GameClassInfo> gameClassesInfo;
		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
		public Observable<GameClass> ChangeClassButtonPressed { get; private set; }
		public Observable<Unit> ExitButtonPressed { get; private set; }

		private IStaticDataService _staticData;

		[Inject]
		private void Construct(IStaticDataService staticData)
		{
			_staticData = staticData;
		}

		public void Initialize()
		{
			GameClass[] playableGameClasses = {
				GameClass.Builder,
				GameClass.Sniper,
				GameClass.Grenadier,
				GameClass.Combatant
			};

			ExitButtonPressed = exitButton.onClick.AsObservable();
			ChangeClassButtonPressed = Observable.Empty<GameClass>();
			
			for (var i = 0; i < playableGameClasses.Length; i++)
			{
				GameClass gameClass = playableGameClasses[i];
				gameClassesInfo[i].SetClassName(gameClass);

				Characteristics characteristics = _staticData.GetCharacteristics(gameClass);
				var itemTypes = _staticData.GetItems(gameClass);
				gameClassesInfo[i].SetHealthValue(characteristics.MaxHealth);

				SetupWeapon(gameClassesInfo[i], itemTypes[0], true);
				SetupWeapon(gameClassesInfo[i], itemTypes[1], false);

				gameClassesInfo[i].SetMeleeWeaponIcon(_staticData.GetSlotIcon(itemTypes[2]));

				ChangeClassButtonPressed = Observable.Merge(ChangeClassButtonPressed,
					gameClassesInfo[i].ChooseClassButton.onClick
						.AsObservable()
						.Select(_ => gameClass));
			}
		}

		private void SetupWeapon(GameClassInfo info, ItemType itemType, bool isMain)
		{
			var itemConfigure = _staticData.GetItemConfigure<InventoryItemConfigure>(itemType);

			if (itemConfigure is RangeWeaponConfigure rangeWeaponConfigure)
			{
				info.SetMainWeapon(_staticData.GetSlotIcon(itemType), _staticData.GetProjectileIcon(itemType),
					$"{rangeWeaponConfigure.MagazineSize}/{rangeWeaponConfigure.TotalBullets}", isMain);
			}

			if (itemConfigure is DrillLauncherConfigure drillLauncherConfigure)
			{
				info.SetMainWeapon(_staticData.GetSlotIcon(itemType), _staticData.GetProjectileIcon(itemType),
					drillLauncherConfigure.Amount.ToString(), isMain);
			}

			if (itemConfigure is RocketLauncherConfigure rocketLauncherConfigure)
			{
				info.SetMainWeapon(_staticData.GetSlotIcon(itemType), _staticData.GetProjectileIcon(itemType),
					rocketLauncherConfigure.Amount.ToString(), isMain);
			}
		}
	}
}