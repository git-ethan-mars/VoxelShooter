namespace Data
{
    public class BlockItemData : IMutableItemData
    {
        public int Amount { get; set; }
        public BlockItemData(BlockItem block)
        {
            Amount = block.count;
        }
    }
}