using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public interface IAvatarLoader : IService
    {
        Dictionary<CSteamID, Texture2D> AvatarBySteamId { get; }
        void RequestAvatar(CSteamID steamID);
    }
}