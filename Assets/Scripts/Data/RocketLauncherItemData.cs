namespace Data
{
    public class RocketLauncherItemData : IMutableItemData
    {
        public int CarriedRockets { get; set; }
        public int ChargedRockets { get; set; }
        public bool IsReloading { get; set; }

        public RocketLauncherItemData(RocketLauncherItem rocketLauncher)
        {
            CarriedRockets = rocketLauncher.count;
            ChargedRockets = rocketLauncher.chargedRocketsCapacity;
            IsReloading = false;
        }
    }
}