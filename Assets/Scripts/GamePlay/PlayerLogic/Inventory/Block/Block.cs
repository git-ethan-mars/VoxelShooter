using Common.StaticData;

namespace Inventory.Block
{
	public class Block : IInventoryItem
	{
		public BlockItemData Data { get; private set; }

		public void Construct(BlockItemData data)
		{
			Data = data;
		}

		public void Enable()
		{
			throw new System.NotImplementedException();
		}

		public void Disable()
		{
			throw new System.NotImplementedException();
		}
	}
}