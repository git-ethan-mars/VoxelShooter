using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public class SteamAvatarLoader : IAvatarLoader
    {
        public Texture2D LoadAvatar(CSteamID steamID)
        {
            var avatarHandle = SteamFriends.GetLargeFriendAvatar(steamID);
            if (!SteamUtils.GetImageSize(avatarHandle, out var avatarWidth, out var avatarHeight))
            {
                Debug.Log("Can't get avatar size");
            }

            var avatarSizeInBytes = (int) (avatarWidth * avatarHeight * 4);
            var avatarBuffer = new byte[avatarSizeInBytes];
            if (!SteamUtils.GetImageRGBA(avatarHandle, avatarBuffer, avatarSizeInBytes))
            {
                Debug.Log("Can't download avatar");
            }

            var texture = new Texture2D((int) avatarWidth, (int) avatarHeight, TextureFormat.RGBA32, false, true);
            texture.LoadRawTextureData(avatarBuffer);
            texture.Apply();
            return texture;
        }
    }
}