using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class BlockInventoryDrawer : MonoBehaviour
    {
        [SerializeField] private Texture2D _texture2D;

        private void Start()
        {
            var uniqueColors = new HashSet<Color>();
            foreach (var color in _texture2D.GetPixels())
            {
                uniqueColors.Add(color);
            }

            foreach (var color in uniqueColors)
            {
                Debug.Log(color);
            }
        }
    }
}