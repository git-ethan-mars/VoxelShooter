using GamePlay;

namespace UI.Inventory
{
	public class BlockPresenter : SlotPresenter
	{
		private readonly Block _block;
		private readonly PaletteView _paletteView;
		private readonly Hud _hud;
		
		public BlockPresenter(Block block, SlotView slotView, Hud hud, PaletteView paletteView) : base(block, slotView)
		{
			_block = block;
			_hud = hud;
			_paletteView = paletteView;
		}

		public override void Initialize()
		{
			base.Initialize();
			_block.Selected += OnSelected;
			_block.Deselected += OnDeselected;
			_block.Data.AmountChanged += OnAmountChanged;
		}

		public override void Dispose()
		{
			base.Dispose();
			_block.Selected -= OnSelected;
			_block.Deselected -= OnDeselected;
			_block.Data.AmountChanged -= OnAmountChanged;
		}

		private void OnSelected()
		{
			_paletteView.Show();
			_hud.ShowItemInfo(_block.InventoryIcon, _block.Data.Amount.ToString());
		}

		private void OnDeselected()
		{
			_paletteView.Hide();
			_hud.HideItemInfo();
		}

		private void OnAmountChanged(int amount)
		{
			_hud.SetItemCount(amount.ToString());
		}
	}
}