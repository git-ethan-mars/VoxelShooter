using System;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public interface IAvatarLoader : IService
    {
        public event Action<CSteamID, Texture2D> OnAvatarLoaded;
        void RequestAvatar(CSteamID steamID);
    }
}