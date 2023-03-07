using UnityEngine;
using UnityEngine.Profiling;

namespace Core
{
    public class GameSettings : MonoBehaviour
    {
        public void Awake()
        {
            Profiler.maxUsedMemory = int.MaxValue;
            Application.targetFrameRate = 60;
        }
    }
}