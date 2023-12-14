using Data;
using Mirror;
using Networking.Messages.Requests;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class ShootHandler : RequestHandler<ShootRequest>
    {
        private readonly IServer _server;
        private readonly RangeWeaponValidator _weaponValidator;
        private readonly RocketLauncherValidator _rocketLauncherValidator;

        public ShootHandler(IServer server, RangeWeaponValidator weaponValidator,
            RocketLauncherValidator rocketLauncherValidator)
        {
            _server = server;
            _weaponValidator = weaponValidator;
            _rocketLauncherValidator = rocketLauncherValidator;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, ShootRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (result && playerData.IsAlive)
            {
                if (playerData.SelectedItem is RangeWeaponItem)
                {
                    _weaponValidator.Shoot(connection, request.Ray, request.IsButtonHolding);
                }

                if (playerData.SelectedItem is RocketLauncherItem)
                {
                    _rocketLauncherValidator.Shoot(connection, request.Ray);
                }
            }
        }
    }
}