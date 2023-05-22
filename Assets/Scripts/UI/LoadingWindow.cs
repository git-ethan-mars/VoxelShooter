using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoadingWindow : MonoBehaviour
    {
        [SerializeField] private List<Image> bullets;
        private float _previousProgress;
    
        public void Construct(CustomNetworkManager networkManager)
        {
            networkManager.OnLoadProgress += UpdateBullets;
        }
        private void UpdateBullets(float progress)
        {
            var startBulletIndex = (int) Math.Floor(_previousProgress * bullets.Count);
            var endBulletIndex = (int) Math.Ceiling(progress * bullets.Count);
            for (var i = startBulletIndex; i < endBulletIndex; i++)
            {
                var currentColor = bullets[i].color;
                bullets[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 255);
            }

            _previousProgress = progress;
            if (progress == 1)
            {
                Destroy(gameObject);
            }
        }
    }
}
