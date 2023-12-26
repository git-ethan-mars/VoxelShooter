namespace Data
{
    public class GrenadeItemData : IMutableItemData
    {
        public int Count { get; set; }

        public GrenadeItemData(GrenadeItem grenade)
        {
            Count = grenade.count;
        }
    }
}