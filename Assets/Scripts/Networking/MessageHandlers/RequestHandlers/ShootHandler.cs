using Mirror;
using Networking.Messages.Requests;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class ShootHandler : RequestHandler<ShootRequest>
    {
        private readonly IServer _server;
        private readonly RangeWeaponValidator _weaponValidator;

        public ShootHandler(IServer server, RangeWeaponValidator weaponValidator)
        {
            _server = server;
            _weaponValidator = weaponValidator;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, ShootRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            _weaponValidator.Shoot(connection, request.Ray, request.IsButtonHolding);
        }
    }
}