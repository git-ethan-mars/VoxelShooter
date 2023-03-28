using UnityEngine;

namespace Infrastructure
{
    public class GameRunner : MonoBehaviour
    {
        public GameBootstrapper bootstrapperPrefab;

        private void Awake()
        {
            if (FindObjectOfType<GameBootstrapper>() is null)
                Instantiate(bootstrapperPrefab);
        }
    }
}