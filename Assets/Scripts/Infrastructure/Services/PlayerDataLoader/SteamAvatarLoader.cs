using System;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class SteamAvatarLoader : IAvatarLoader
    {
        protected Callback<AvatarImageLoaded_t> AvatarLoaded;
        public event Action<CSteamID, Texture2D> OnAvatarLoaded;

        public SteamAvatarLoader()
        {
            AvatarLoaded = new Callback<AvatarImageLoaded_t>(LoadAvatarSlow);
        }

        public void RequestAvatar(CSteamID steamID)
        {
            var avatarHandle = SteamFriends.GetLargeFriendAvatar(steamID);
            if (avatarHandle == 0) Debug.Log("User has no avatar");
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
            OnAvatarLoaded?.Invoke(avatarImageResult.m_steamID, texture);
        }
    }
}