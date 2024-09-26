using Common.StaticData;
using Mirror;
using Networking.Messages.Requests;

namespace Networking.Host
{
    public partial class MirrorHost : IRequestHandler<ShootRequest>, IRequestHandler<ReloadRequest>
    {
        public void OnRequestReceived(NetworkConnectionToClient connection, ShootRequest request)
        {
            var result = TryGetPlayerData(connection, out var playerData);
            if (result && playerData.IsAlive)
            {
                if (playerData.SelectedItemData is RangeWeaponData)
                {
                    _rangeWeaponValidator.Shoot(connection, request.Ray, request.IsButtonHolding);
                }

                if (playerData.SelectedItemData is RangeWeaponData)
                {
                    _rocketLauncherValidator.Shoot(connection, request.Ray);
                }
                
                if (playerData.SelectedItemData is DrillLauncherData)
                {
                    _drillValidator.Shoot(connection, request.Ray);
                }
            }
        }

        public void OnRequestReceived(NetworkConnectionToClient connection, ReloadRequest request)
        {
            var result = TryGetPlayerData(connection, out var playerData);
            if (result && playerData.IsAlive)
            {
                if (playerData.SelectedItemData is RangeWeaponData)
                {
                    _rangeWeaponValidator.Reload(connection);
                }

                if (playerData.SelectedItemData is RocketLauncherData)
                {
                    _rocketLauncherValidator.Reload(connection);
                }
                
                if (playerData.SelectedItemData is DrillLauncherData)
                {
                    _drillValidator.Reload(connection);
                }
            }
        }
    }
}