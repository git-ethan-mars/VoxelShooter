using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IPlayerFactory
    {
        GameObject CreatePlayer();
        GameObject CreateSpectatorPlayer(Vector3 deathPosition);
    }
}