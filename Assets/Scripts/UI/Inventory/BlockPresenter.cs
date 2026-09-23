using System;
using GamePlay;
using R3;
using Services;
using UnityEngine;
namespace UI.Inventory
{
	public class BlockPresenter : SlotPresenter<Block>
	{
		private readonly CharacterProvider _characterProvider;
		private readonly Hud _hud;

		private IDisposable _disposable;
		private Sprite _projectileIcon;

		public BlockPresenter(IStaticDataService staticData, CharacterProvider characterProvider,
			UIProvider uiProvider, Block block, SlotView slotView)
			: base(staticData, block, slotView)
		{
			_characterProvider = characterProvider;
			_hud = uiProvider.Hud;
		}

		public override void Initialize()
		{
			base.Initialize();

			_disposable = _characterProvider.Character.Value.Inventory.VoxelAmount.Subscribe(OnAmountChanged);
			_projectileIcon = StaticData.GetProjectileIcon(InventoryItem.Type);
		}

		public override void Select()
		{
			base.Select();

			_hud.ShowPalette();
			_hud.ShowItemInfo(_projectileIcon, _characterProvider.Character.Value.Inventory.VoxelAmount.CurrentValue.ToString());
			_hud.SetCrosshairVisibility(true);
		}

		public override void Deselect()
		{
			base.Deselect();

			_hud.HidePalette();
			_hud.HideItemInfo();
			_hud.SetCrosshairVisibility(false);
		}

		public override void Dispose()
		{
			base.Dispose();

			_disposable.Dispose();
		}

		private void OnAmountChanged(int amount)
		{
			if (InventoryItem.IsSelected)
			{
				_hud.SetItemCount(amount.ToString());
			}
		}
	}
}