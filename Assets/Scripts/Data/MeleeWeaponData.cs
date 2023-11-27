using UnityEngine;

namespace Data
{
    public class MeleeWeaponData
    {
        public readonly float Range;
        public readonly float TimeBetweenHit;

        public readonly int DamageToPlayer;
        public readonly int DamageToBlock;

        public readonly int ID;
        public readonly GameObject Prefab;
        public readonly Sprite Icon;

        public readonly float HeadMultiplier;
        public readonly float ChestMultiplier;
        public readonly float LegMultiplier;
        public readonly float ArmMultiplier;

        public AudioData DiggingAudio;
        public AudioData HittingAudio;

        public bool IsReady;
        public bool HasStrongHit;

        public MeleeWeaponData(MeleeWeaponItem meleeWeapon)
        {
            Range = meleeWeapon.range;
            DamageToPlayer = meleeWeapon.damageToPlayer;
            DamageToBlock = meleeWeapon.damageToBlock;
            ID = meleeWeapon.id;
            HeadMultiplier = meleeWeapon.headMultiplier;
            ChestMultiplier = meleeWeapon.chestMultiplier;
            LegMultiplier = meleeWeapon.legMultiplier;
            ArmMultiplier = meleeWeapon.armMultiplier;
            DiggingAudio = meleeWeapon.diggingAudio;
            HittingAudio = meleeWeapon.diggingAudio;
            TimeBetweenHit = meleeWeapon.timeBetweenHit;
            Prefab = meleeWeapon.prefab;
            Icon = meleeWeapon.inventoryIcon;
            HasStrongHit = meleeWeapon.hasStrongHit;
            IsReady = true;
        }
    }
}