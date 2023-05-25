using System;
using Infrastructure.AssetManagement;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class LocalAvatarLoader : IAvatarLoader
    {
        private const string SteamAvatarPath = "Sprites/steam-question-mark";
        private readonly IAssetProvider _assets;

        public LocalAvatarLoader(IAssetProvider assets)
        {
            _assets = assets;
        }

        public event Action<CSteamID, Texture2D> OnAvatarLoaded;
        public void RequestAvatar(CSteamID steamID)
        {
            OnAvatarLoaded?.Invoke(steamID, _assets.Load<Texture2D>(SteamAvatarPath));
        }
    }
}