using Steamworks;
using UnityEngine;

namespace Infrastructure.Services.PlayerDataLoader
{
    public interface IAvatarLoader : IService
    {
        Texture2D LoadAvatar(CSteamID steamID);
    }
}