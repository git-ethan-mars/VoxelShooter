using Data;
using Entities;
using Explosions;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class GrenadeSpawnHandler : RequestHandler<GrenadeSpawnRequest>
    {
        private readonly IServer _server;
        private readonly IEntityFactory _entityFactory;
        private readonly ExplosionBehaviour _explosionBehaviour;
        private readonly AudioService _audioService;
        private readonly ICoroutineRunner _coroutineRunner;

        public GrenadeSpawnHandler(IServer server, CustomNetworkManager networkManager, AudioService audioService)
        {
            _server = server;
            _entityFactory = networkManager.EntityFactory;
            _audioService = audioService;
            _coroutineRunner = networkManager;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, GrenadeSpawnRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive || playerData.SelectedItem is not GrenadeItem grenadeItem)
            {
                return;
            }

            var grenadeData = (GrenadeItemData) playerData.ItemData[playerData.SelectedSlotIndex];
            if (grenadeData.Amount <= 0)
            {
                return;
            }

            grenadeData.Amount -= 1;
            connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex, grenadeData.Amount - 1));
            var force = request.Ray.direction * request.ThrowForce;
            var grenade = _entityFactory.CreateGrenade(request.Ray.origin, force, _server, connection, grenadeItem,
                _audioService);
            NetworkServer.Spawn(grenade.gameObject);
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => Explode(grenade),
                grenadeItem.delayInSeconds));
        }

        private void Explode(Grenade grenade)
        {
            if (grenade != null)
            {
                grenade.Explode();
            }
        }
    }
}