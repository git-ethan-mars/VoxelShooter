using System.Collections.Generic;
using Infrastructure.AssetManagement;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class SteamAvatarLoader : IAvatarLoader
    {
        protected Callback<AvatarImageLoaded_t> AvatarLoaded;
        private readonly Dictionary<CSteamID, Texture2D> _avatarBySteamId;
        private readonly IAssetProvider _assets;
        private const string SteamAvatarPath = "Sprites/steam-question-mark";


        public SteamAvatarLoader(IAssetProvider assets)
        {
            _assets = assets;
            AvatarLoaded = new Callback<AvatarImageLoaded_t>(LoadAvatarSlow);
            _avatarBySteamId = new Dictionary<CSteamID, Texture2D>();
        }

        public Texture2D RequestAvatar(CSteamID steamID)
        {
            if (_avatarBySteamId.TryGetValue(steamID, out var texture)) return texture;
            var avatarHandle = SteamFriends.GetLargeFriendAvatar(steamID);
            texture = avatarHandle <= 0 ? _assets.Load<Texture2D>(SteamAvatarPath) : LoadAvatarFast(avatarHandle);
            _avatarBySteamId[steamID] = texture;
            return texture;
        }

        private Texture2D LoadAvatarFast(int imageHandle)
        {
            if (!SteamUtils.GetImageSize(imageHandle, out var avatarWidth, out var avatarHeight))
            {
                Debug.Log("Can't get avatar size");
            }

            var avatarSizeInBytes = (int) (avatarWidth * avatarHeight * 4);
            var avatarBuffer = new byte[avatarSizeInBytes];
            if (!SteamUtils.GetImageRGBA(imageHandle, avatarBuffer, avatarSizeInBytes))
            {
                Debug.Log("Can't download avatar");
            }
            var texture = new Texture2D((int) avatarWidth, (int) avatarHeight, TextureFormat.RGBA32, false, true);
            texture.LoadRawTextureData(avatarBuffer);
            texture.Apply();
            return texture;
        }

        private void LoadAvatarSlow(AvatarImageLoaded_t avatarImageResult)
        {
            if (!SteamUtils.GetImageSize(avatarImageResult.m_iImage, out var avatarWidth, out var avatarHeight))
            {
                Debug.Log("Can't get avatar size");
            }

            var avatarSizeInBytes = (int) (avatarWidth * avatarHeight * 4);
            var avatarBuffer = new byte[avatarSizeInBytes];
            if (!SteamUtils.GetImageRGBA(avatarImageResult.m_iImage, avatarBuffer, avatarSizeInBytes))
            {
                Debug.Log("Can't download avatar");
            }

            var texture = new Texture2D((int) avatarWidth, (int) avatarHeight, TextureFormat.RGBA32, false, true);
            texture.LoadRawTextureData(avatarBuffer);
            texture.Apply();
            _avatarBySteamId[avatarImageResult.m_steamID] = texture;
        }
    }
}