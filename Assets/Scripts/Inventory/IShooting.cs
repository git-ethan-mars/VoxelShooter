namespace Inventory
{
    public interface IShooting
    {
        public int BulletsInMagazine { get; set; }
        public void OnShootResult();
    }
}