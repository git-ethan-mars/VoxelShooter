using Mirror;
using Networking.Messages.Requests;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class HitHandler : RequestHandler<HitRequest>
    {
        private readonly IServer _server;
        private readonly MeleeWeaponValidator _weaponValidator;

        public HitHandler(IServer server, MeleeWeaponValidator weaponValidator)
        {
            _server = server;
            _weaponValidator = weaponValidator;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, HitRequest request)
        {
            var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive)
            {
                return;
            }

            _weaponValidator.Hit(connection, request.Ray, request.IsStrongHit);
        }
    }
}