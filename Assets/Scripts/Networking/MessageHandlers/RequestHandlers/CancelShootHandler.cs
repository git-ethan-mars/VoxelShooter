using Mirror;
using Networking.Messages.Requests;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class CancelShootHandler : RequestHandler<CancelShootRequest>
    {
        private readonly Server _server;
        private readonly RangeWeaponValidator _weaponValidator;

        public CancelShootHandler(Server server, RangeWeaponValidator weaponValidator)
        {
            _server = server;
            _weaponValidator = weaponValidator;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, CancelShootRequest request)
        {
            var result = _server.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive || !playerData.HasContinuousSound)
            {
                return;
            }

            _weaponValidator.CancelShoot(connection);
        }
    }
}