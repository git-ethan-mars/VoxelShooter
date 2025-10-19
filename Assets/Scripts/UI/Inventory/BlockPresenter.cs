using System;
using GamePlay;
using R3;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class BlockPresenter : SlotPresenter<Block>
	{
		private readonly Hud _hud;

		private IDisposable _disposable;
		private Sprite _projectileIcon;

		public BlockPresenter(IStaticDataService staticData, UIProvider uiProvider, Block block, SlotView slotView)
			: base(staticData, block, slotView)
		{
			_hud = uiProvider.InGameUI.Hud;
		}

		public override void Initialize()
		{
			base.Initialize();

			_disposable = InventoryItem.Amount.Subscribe(value => _hud.SetItemCount(value.ToString()));
			_projectileIcon = StaticData.GetProjectileIcon(InventoryItem.Type);
		}

		public override void Dispose()
		{
			base.Dispose();
			
			_disposable.Dispose();
		}

		protected override void OnSelected()
		{
			base.OnSelected();
			
			_hud.ShowPalette();
			_hud.ShowItemInfo(_projectileIcon, InventoryItem.Amount.ToString());
		}

		protected override void OnDeselected()
		{
			base.OnDeselected();
			
			_hud.HidePalette();
			_hud.HideItemInfo();
		}
	}
}