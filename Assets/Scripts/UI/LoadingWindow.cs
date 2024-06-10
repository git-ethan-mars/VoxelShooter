using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LoadingWindow : MonoBehaviour
    {
        [SerializeField]
        private List<Image> bullets;

        private float _previousProgress;
        private CustomNetworkManager _networkManager;
        private IClient _client;

        public void Construct(CustomNetworkManager networkManager)
        {
            _networkManager = networkManager;
        }

        private void Update()
        {
            if (_networkManager.Client != null)
            {
                _client = _networkManager.Client;
                _client.MapLoadingProgress.LoadProgress.ValueChanged += UpdateLoadingBar;
                enabled = false;
            }
        }

        private void UpdateLoadingBar(float progress)
        {
            var startBulletIndex = (int) Math.Floor(_previousProgress * bullets.Count);
            var endBulletIndex = (int) Math.Ceiling(progress * bullets.Count);
            for (var i = startBulletIndex; i < endBulletIndex; i++)
            {
                var currentColor = bullets[i].color;
                bullets[i].color = new Color(currentColor.r, currentColor.g, currentColor.b, 255);
            }

            _previousProgress = progress;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (progress == 1)
            {
                Destroy(gameObject);
            }
        }

        public void OnDestroy()
        {
            _client.MapLoadingProgress.LoadProgress.ValueChanged -= UpdateLoadingBar;
        }
    }
}