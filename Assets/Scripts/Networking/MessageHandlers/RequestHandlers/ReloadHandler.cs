using Data;
using Mirror;
using Networking.Messages.Requests;
using Networking.ServerServices;
using UnityEngine;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class ReloadHandler : RequestHandler<ReloadRequest>
    {
        private readonly IServer _server;
        private readonly RangeWeaponValidator _rangeWeaponValidator;
        private readonly RocketLauncherValidator _rocketLauncherValidator;
        private readonly DrillValidator _drillValidator;

        public ReloadHandler(IServer server, RangeWeaponValidator rangeWeaponValidator,
            RocketLauncherValidator rocketLauncherValidator, DrillValidator drillValidator)
        {
            _server = server;
            _rangeWeaponValidator = rangeWeaponValidator;
            _rocketLauncherValidator = rocketLauncherValidator;
            _drillValidator = drillValidator;
        }
        protected override void OnRequestReceived(NetworkConnectionToClient connection, ReloadRequest request)
        {
            var result = _server.TryGetPlayerData(connection, out var playerData);
            if (result && playerData.IsAlive)
            {
                if (playerData.SelectedItem is RangeWeaponItem)
                {
                    _rangeWeaponValidator.Reload(connection);
                }

                if (playerData.SelectedItem is RocketLauncherItem)
                {
                    _rocketLauncherValidator.Reload(connection);
                }
                
                if (playerData.SelectedItem is DrillItem)
                {
                    _drillValidator.Reload(connection);
                }
            }
        }
    }
}