using Mirror;
using UnityEngine;

namespace Networking.ServerServices
{
    public interface IRangeWeaponValidator
    {
        void Shoot(NetworkConnectionToClient connection, Ray ray, bool requestIsButtonHolding, int tick);
        void CancelShoot(NetworkConnectionToClient connection);
        void Reload(NetworkConnectionToClient connection);
    }
}