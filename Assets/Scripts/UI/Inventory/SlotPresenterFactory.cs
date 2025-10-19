using GamePlay.Core;
namespace UI.Inventory
{
	public class SlotPresenterFactory : ISlotPresenterFactory
	{
		private readonly ISlotPresenterRegistry _registry;

		public SlotPresenterFactory(ISlotPresenterRegistry registry)
		{
			_registry = registry;
		}

		public SlotPresenter CreatePresenter(InventoryItem item, SlotView slotView)
		{
			return _registry.Create(item, slotView);
		}
	}

}