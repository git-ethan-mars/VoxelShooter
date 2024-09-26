using Common.StaticData;
using Mirror;
using Networking.Messages.Requests;

namespace Networking.Host
{
    public partial class MirrorHost : IRequestHandler<HitRequest>
    {
        public void OnRequestReceived(NetworkConnectionToClient connection, HitRequest request)
        {
            var result = TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive || playerData.SelectedItemData is not MeleeWeaponData)
            {
                return;
            }

            _meleeWeaponValidator.Hit(connection, request.Ray, request.IsStrongHit);
        }
    }
}