namespace Inventory
{
    public interface IReloading
    {
        public int BulletsInMagazine { get; set; }
        public int TotalBullets { get; set; }
        public void OnReloadResult();
    }
}