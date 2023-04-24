using Unity.VisualScripting;
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
        
        public AudioClip DiggingAudioClip;
        public float DiggingVolume;
        public AudioClip HitAudioClip;
        public float HitVolume;
        
        public bool IsReady;
        public bool IsReloading;
        
        public MeleeWeaponData(MeleeWeaponItem primaryMeleeWeapon)
        {
            Range = primaryMeleeWeapon.range;
            DamageToPlayer = primaryMeleeWeapon.damageToPlayer;
            DamageToBlock = primaryMeleeWeapon.damageToBlock;
            ID = primaryMeleeWeapon.id;
            HeadMultiplier = primaryMeleeWeapon.headMultiplier;
            ChestMultiplier = primaryMeleeWeapon.chestMultiplier;
            LegMultiplier = primaryMeleeWeapon.legMultiplier;
            ArmMultiplier = primaryMeleeWeapon.armMultiplier;
            DiggingAudioClip = primaryMeleeWeapon.diggingAudioClip;
            DiggingVolume = primaryMeleeWeapon.diggingVolume;
            HitAudioClip = primaryMeleeWeapon.hitAudioClip;
            HitVolume = primaryMeleeWeapon.hitVolume;
            TimeBetweenHit = primaryMeleeWeapon.timeBetweenHit;
            Prefab = primaryMeleeWeapon.prefab;
            Icon = primaryMeleeWeapon.inventoryIcon;
            IsReady = true;
        }
    }
}