using System.Collections.Generic;
using Infrastructure.AssetManagement;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class LocalAvatarLoader : IAvatarLoader
    {
        private const string SteamAvatarPath = "Sprites/steam-question-mark";
        private readonly Dictionary<CSteamID, Texture2D> _avatarBySteamId;
        private readonly IAssetProvider _assets;


        public LocalAvatarLoader(IAssetProvider assets)
        {
            _assets = assets;
            _avatarBySteamId = new Dictionary<CSteamID, Texture2D>();
        }

        public Texture2D RequestAvatar(CSteamID steamID)
        {
            if (!_avatarBySteamId.TryGetValue(steamID, out var texture))
            {
                texture = _assets.Load<Texture2D>(SteamAvatarPath);
                _avatarBySteamId[steamID] = texture;
            }

            return texture;

        }
    }
}