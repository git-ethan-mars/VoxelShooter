using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Grenade", menuName = "Inventory System/Inventory Items/Grenade")]

    public class GrenadeItem : InventoryItem
    {
        public Sprite countIcon;
        public int count;
        public float delayInSeconds;
        public int radius;
        public int damage;
        public float maxThrowDuration;
        public float throwForceModifier;
        public float minThrowForce;
        public int particlesSpeed;
        public int particlesCount;

        public void Awake()
        {
            itemType = ItemType.Grenade;
        }
    }
}