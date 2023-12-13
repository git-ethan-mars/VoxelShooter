using Data;
using Mirror;
using Networking.Messages.Requests;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class ReloadHandler : RequestHandler<ReloadRequest>
    {
        private readonly IServer _server;
        private readonly RangeWeaponValidator _weaponValidator;

        public ReloadHandler(IServer server, RangeWeaponValidator weaponValidator)
        {
            _server = server;
            _weaponValidator = weaponValidator;
        }
        protected override void OnRequestReceived(NetworkConnectionToClient connection, ReloadRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive || playerData.SelectedItem is not RangeWeaponItem)
            {
                return;
            }

            _weaponValidator.Reload(connection);
        }
    }
}