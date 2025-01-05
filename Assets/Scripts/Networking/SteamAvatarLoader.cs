using System.Collections.Generic;
using Common;
using Common.AssetManagement;
using Steamworks;
using UnityEngine;

namespace Networking
{
    public class SteamAvatarLoader : IAvatarLoader
    {
        protected Callback<AvatarImageLoaded_t> AvatarLoaded;
        private readonly Dictionary<ulong, Texture2D> _avatarBySteamId;
        private readonly IAssetProvider _assets;
        private const string SteamAvatarPath = "Sprites/steam-question-mark";


        public SteamAvatarLoader(IAssetProvider assets)
        {
            _assets = assets;
            AvatarLoaded = Callback<AvatarImageLoaded_t>.Create(LoadAvatarSlow);
            _avatarBySteamId = new Dictionary<ulong, Texture2D>();
        }

        public Texture2D RequestAvatar(ulong playerId)
        {
            if (_avatarBySteamId.TryGetValue(playerId, out var texture))
            {
                return texture;
            }
            
            var avatarHandle = SteamFriends.GetLargeFriendAvatar(new CSteamID(playerId));
            texture = avatarHandle <= 0 ? _assets.Load<Texture2D>(SteamAvatarPath) : LoadAvatarFast(avatarHandle);
            _avatarBySteamId[playerId] = texture;
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
            _avatarBySteamId[avatarImageResult.m_steamID.m_SteamID] = texture;
        }
    }
}