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

        public ReloadHandler(IServer server, RangeWeaponValidator rangeWeaponValidator, RocketLauncherValidator rocketLauncherValidator)
        {
            _server = server;
            _rangeWeaponValidator = rangeWeaponValidator;
            _rocketLauncherValidator = rocketLauncherValidator;
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
            }
        }
    }
}