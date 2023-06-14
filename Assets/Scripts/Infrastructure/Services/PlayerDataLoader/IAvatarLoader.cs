using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public interface IAvatarLoader : IService
    {
        Texture2D RequestAvatar(CSteamID steamID);
    }
}