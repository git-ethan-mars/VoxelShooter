using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace Core
{
    public class GameSettings : MonoBehaviour
    {
        public void Awake()
        {
            Application.targetFrameRate = 60;
        }
    }
}