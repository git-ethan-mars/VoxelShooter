using UnityEngine;

namespace Infrastructure.Factory
{
    public interface IPlayerFactory
    {
        GameObject CreatePlayer(Vector3 position);
        GameObject CreateSpectatorPlayer(Vector3 deathPosition);
    }
}