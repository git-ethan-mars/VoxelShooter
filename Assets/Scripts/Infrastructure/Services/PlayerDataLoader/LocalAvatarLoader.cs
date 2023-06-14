using System.Collections.Generic;
using Infrastructure.AssetManagement;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class LocalAvatarLoader : IAvatarLoader
    {
        private const string SteamAvatarPath = "Sprites/steam-question-mark";
        private readonly Dictionary<CSteamID, Texture2D> AvatarBySteamId;
        private readonly IAssetProvider _assets;


        public LocalAvatarLoader(IAssetProvider assets)
        {
            _assets = assets;
            AvatarBySteamId = new Dictionary<CSteamID, Texture2D>();
        }

        public Texture2D RequestAvatar(CSteamID steamID)
        {
            if (!AvatarBySteamId.TryGetValue(steamID, out var texture))
            {
                texture = _assets.Load<Texture2D>(SteamAvatarPath);
                AvatarBySteamId[steamID] = texture;
            }

            return texture;

        }
    }
}