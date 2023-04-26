using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Melee Weapon", menuName = "Inventory System/Inventory Items/Melee weapon")]
    public class MeleeWeaponItem : InventoryItem
    {
        [Header("Configuration")] 
        public float range;
        public float timeBetweenHit;
        
        [Header("Damage")]
        public int damageToPlayer;
        [Range(0,5)]
        public float headMultiplier;
        [Range(0,5)]
        public float chestMultiplier;
        [Range(0,5)]
        public float legMultiplier;
        [Range(0,5)]
        public float armMultiplier;
        public int damageToBlock;
        
        [Header("Audio")]
        public AudioClip diggingAudioClip;
        [Range(0, 1)] public float diggingVolume;
        public AudioClip hitAudioClip;
        [Range(0, 1)] public float hitVolume;
        
        private void Awake()
        {
            itemType = ItemType.MeleeWeapon;
        }
    }
}