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
        private readonly DrillValidator _drillValidator;

        public ShootHandler(IServer server, RangeWeaponValidator weaponValidator,
            RocketLauncherValidator rocketLauncherValidator, DrillValidator drillValidator)
        {
            _server = server;
            _weaponValidator = weaponValidator;
            _rocketLauncherValidator = rocketLauncherValidator;
            _drillValidator = drillValidator;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, ShootRequest request)
        {
            var result = _server.TryGetPlayerData(connection, out var playerData);
            if (result && playerData.IsAlive)
            {
                if (playerData.SelectedItem is RangeWeaponItem)
                {
                    _weaponValidator.Shoot(connection, request.Ray, request.IsButtonHolding, request.Tick);
                }

                if (playerData.SelectedItem is RocketLauncherItem)
                {
                    _rocketLauncherValidator.Shoot(connection, request.Ray);
                }
                
                if (playerData.SelectedItem is DrillItem)
                {
                    _drillValidator.Shoot(connection, request.Ray);
                }
            }
        }
    }
}