namespace Data
{
    public class TntItemData : IMutableItemData
    {
        public int Amount { get; set; }
        public TntItemData(TntItem tnt)
        {
            Amount = tnt.count;
        }
    }
}