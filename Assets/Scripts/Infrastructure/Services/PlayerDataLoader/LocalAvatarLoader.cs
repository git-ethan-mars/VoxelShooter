using System.Collections.Generic;
using Infrastructure.AssetManagement;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class LocalAvatarLoader : IAvatarLoader
    {
        private const string SteamAvatarPath = "Sprites/steam-question-mark";
        private readonly IAssetProvider _assets;

        public Dictionary<CSteamID, Texture2D> AvatarBySteamId { get; }

        public LocalAvatarLoader(IAssetProvider assets)
        {
            _assets = assets;
            AvatarBySteamId = new Dictionary<CSteamID, Texture2D>();
        }

        public void RequestAvatar(CSteamID steamID)
        {
            var texture = _assets.Load<Texture2D>(SteamAvatarPath);
            AvatarBySteamId[steamID] = texture;
        }
    }
}