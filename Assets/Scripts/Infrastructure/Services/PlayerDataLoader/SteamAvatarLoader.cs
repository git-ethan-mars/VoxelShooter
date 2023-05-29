using System;
using System.Collections.Generic;
using Infrastructure.AssetManagement;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class SteamAvatarLoader : IAvatarLoader
    {
        public event Action OnAvatarLoaded;
        public Dictionary<CSteamID, Texture2D> AvatarBySteamId { get; set; }
        protected Callback<AvatarImageLoaded_t> AvatarLoaded;
        private readonly IAssetProvider _assets;
        private const string SteamAvatarPath = "Sprites/steam-question-mark";


        public SteamAvatarLoader(IAssetProvider assets)
        {
            _assets = assets;
            AvatarLoaded = new Callback<AvatarImageLoaded_t>(LoadAvatarSlow);
            AvatarBySteamId = new Dictionary<CSteamID, Texture2D>();
        }

        public void RequestAvatar(CSteamID steamID)
        {
            var avatarHandle = SteamFriends.GetLargeFriendAvatar(steamID);
            if (avatarHandle <= 0)
            {
                var texture = _assets.Load<Texture2D>(SteamAvatarPath);
                AvatarBySteamId[steamID] = texture;
                OnAvatarLoaded?.Invoke();
            }
            else
                LoadAvatarFast(steamID, avatarHandle);
        }

        private void LoadAvatarFast(CSteamID steamID, int imageHandle)
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
            AvatarBySteamId[steamID] = texture;
            OnAvatarLoaded?.Invoke();
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
            AvatarBySteamId[avatarImageResult.m_steamID] = texture;
            OnAvatarLoaded?.Invoke();
        }
    }
}