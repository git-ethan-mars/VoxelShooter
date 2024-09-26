using System.Collections.Generic;
using Common.AssetManagement;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class LocalAvatarLoader : IAvatarLoader
    {
        private const string SteamAvatarPath = "Sprites/steam-question-mark";
        private readonly Dictionary<ulong, Texture2D> _avatarBySteamId;
        private readonly IAssetProvider _assets;


        public LocalAvatarLoader(IAssetProvider assets)
        {
            _assets = assets;
            _avatarBySteamId = new Dictionary<ulong, Texture2D>();
        }

        public Texture2D RequestAvatar(ulong playerId)
        {
            if (!_avatarBySteamId.TryGetValue(playerId, out var texture))
            {
                texture = _assets.Load<Texture2D>(SteamAvatarPath);
                _avatarBySteamId[playerId] = texture;
            }

            return texture;

        }
    }
}