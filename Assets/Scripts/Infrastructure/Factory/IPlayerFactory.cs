using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IPlayerFactory
    {
        GameObject CreatePlayer();
        GameObject CreateSpectatorPlayer();
    }
}