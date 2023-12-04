namespace Data
{
    public class MeleeWeaponData : IMutableItemData
    {
        public bool IsReady;

        public MeleeWeaponData(MeleeWeaponItem meleeWeapon)
        {
            IsReady = true;
        }
    }
}