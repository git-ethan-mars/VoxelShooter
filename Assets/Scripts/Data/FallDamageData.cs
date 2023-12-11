using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Fall damage configuration", menuName = "", order = 0)]
    public class FallDamageData : ScriptableObject
    {
        [Header("Minimal speed to damage")]
        public int minSpeedToDamage;

        [Header("Damage multiplier")]
        public int damageMultiplier;
    }
}