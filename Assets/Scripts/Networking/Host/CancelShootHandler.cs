using Mirror;
using Networking.Host.Services;
using Networking.Messages.Requests;

namespace Networking.Host
{
    public partial class MirrorHost : IRequestHandler<CancelShootRequest>
    {
        private readonly RangeWeaponValidator _rangeWeaponValidator;

        public void OnRequestReceived(NetworkConnectionToClient connection, CancelShootRequest request)
        {
            var result = TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive || !playerData.HasContinuousSound)
            {
                return;
            }

            _rangeWeaponValidator.CancelShoot(connection);
        }
    }
}