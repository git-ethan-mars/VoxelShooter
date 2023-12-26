namespace Data
{
    public class BlockItemData : IMutableItemData
    {
        public int Count { get; set; }
        public BlockItemData(BlockItem block)
        {
            Count = block.count;
        }
    }
}