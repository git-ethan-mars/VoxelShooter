using System;
using GamePlay;
using R3;
using Services;
using UnityEngine;

namespace UI.Inventory
{
	public class BlueprintPresenter : SlotPresenter<Blueprint>
	{
		private readonly CharacterProvider _characterProvider;
		private readonly Hud _hud;

		private IDisposable _disposable;
		private Sprite _projectileIcon;

		public BlueprintPresenter(IStaticDataService staticData, CharacterProvider characterProvider,
			UIProvider uiProvider, Blueprint blueprint, SlotView slotView)
			: base(staticData, blueprint, slotView)
		{
			_characterProvider = characterProvider;
			_hud = uiProvider.Hud;
		}

		public override void Initialize()
		{
			base.Initialize();

			_disposable = Disposable.Combine(
				_characterProvider.Character.Value.Inventory.VoxelAmount.Subscribe(_ => OnItemInfoChanged()),
				InventoryItem.LayoutChanged.Subscribe(_ => OnItemInfoChanged()));
			_projectileIcon = StaticData.GetProjectileIcon(InventoryItem.Type);
		}

		public override void Select()
		{
			base.Select();

			_hud.ShowPalette();
			_hud.ShowBlueprintMenu(InventoryItem);
			_hud.ShowItemInfo(_projectileIcon, GetItemInfoText());
			_hud.SetCrosshairVisibility(true);
		}

		public override void Deselect()
		{
			base.Deselect();

			_hud.HidePalette();
			_hud.HideBlueprintMenu();
			_hud.HideItemInfo();
			_hud.SetCrosshairVisibility(false);
		}

		public override void Dispose()
		{
			base.Dispose();

			_disposable.Dispose();
		}

		private void OnItemInfoChanged()
		{
			if (InventoryItem.IsSelected)
			{
				_hud.SetItemCount(GetItemInfoText());
			}
		}

		private string GetItemInfoText()
		{
			int voxelAmount = _characterProvider.Character.Value.Inventory.VoxelAmount.CurrentValue;
			return $"{InventoryItem.CurrentLayout.Name}: {InventoryItem.Positions.Count}/{voxelAmount}";
		}
	}
}
