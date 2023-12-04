using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Player characteristic", menuName = "Balance/Player Characteristic")]
    public class PlayerCharacteristic : ScriptableObject
    {
        public GameClass gameClass;
        public int maxHealth;
        public float speed;
        public float jumpHeight;
        public float placeDistance;
    }
}