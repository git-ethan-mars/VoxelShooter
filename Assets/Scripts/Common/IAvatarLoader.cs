using UnityEngine;

namespace Common
{
    public interface IAvatarLoader : IService
    {
        Texture2D RequestAvatar(ulong playerId);
    }
}