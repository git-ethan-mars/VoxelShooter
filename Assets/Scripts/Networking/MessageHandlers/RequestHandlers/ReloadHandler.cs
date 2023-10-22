using Mirror;
using Networking;
using Networking.MessageHandlers;
using Networking.Messages.Requests;
using Networking.ServerServices;

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
        var result = _server.ServerData.TryGetPlayerData(connection, out var playerData);
        if (!result || !playerData.IsAlive)
        {
            return;
        }

        _weaponValidator.Reload(connection);
    }
}