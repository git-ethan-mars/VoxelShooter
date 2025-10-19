using GamePlay;
using R3;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class TNTPresenter : SlotPresenter<TNT>
	{
		private readonly Hud _hud;
		private Sprite _projectileIcon;

		public TNTPresenter(IStaticDataService staticData, UIProvider uiProvider, TNT tnt, SlotView slotView) : base(staticData, tnt, 
			slotView)
		{
			_hud = uiProvider.InGameUI.Hud;
		}

		public override void Initialize()
		{
			base.Initialize();

			_projectileIcon = StaticData.GetProjectileIcon(InventoryItem.Type);
			InventoryItem.Amount.Subscribe(OnAmountChanged).AddTo(InventoryItem);
		}

		protected override void OnSelected()
		{
			base.OnSelected();

			_hud.ShowItemInfo(_projectileIcon, InventoryItem.Amount.ToString());
		}

		protected override void OnDeselected()
		{
			base.OnDeselected();

			_hud.HideItemInfo();
		}

		private void OnAmountChanged(int amount)
		{
			_hud.SetItemCount(amount.ToString());
		}
	}
}