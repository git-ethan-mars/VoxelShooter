namespace Data
{
    public class RocketLauncherData : IMutableItemData
    {
        public int TotalRockets { get; set; }
        public int RocketsInSlotsCount { get; set; }
        public bool IsReloading { get; set; }

        public RocketLauncherData(RocketLauncherItem rocketLauncher)
        {
            TotalRockets = rocketLauncher.count;
            RocketsInSlotsCount = rocketLauncher.rocketSlotsCount;
            IsReloading = false;
        }
    }
}