namespace Common.StaticData
{
    public class BlockItemData : IItemData
    {
        public int ID => _item.id;
        public int Amount { get; set; }
        private readonly BlockItem _item;

        public BlockItemData(BlockItem item)
        {
            _item = item;
            Amount = item.count;
        }
    }
}