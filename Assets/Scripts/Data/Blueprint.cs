using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "Blueprint", menuName = "Blueprint System/Blueprints/Blueprint")]
    public class Blueprint : ScriptableObject
    {
        public Sprite image;

        public GameObject prefab;
        
        public BlockDataWithPosition[] blockDataWithPositions;
    }
}