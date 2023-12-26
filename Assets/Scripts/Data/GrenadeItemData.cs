namespace Data
{
    public class GrenadeItemData : IMutableItemData
    {
        public int Amount { get; set; }

        public GrenadeItemData(GrenadeItem grenade)
        {
            Amount = grenade.count;
        }
    }
}