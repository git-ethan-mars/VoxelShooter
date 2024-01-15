namespace Data
{
    public class DrillItemData : IMutableItemData
    {
        public int Speed { get; set; }
        public int ChargedDrills { get; set; }
        public bool IsReloading { get; set; }
        public int Amount { get; set; }
        
        public DrillItemData(DrillItem drill)
        {
            Speed = drill.speed;
            Amount = drill.count;
            ChargedDrills = drill.chargedDrillsCapacity;
            IsReloading = false;
        }
    }
}