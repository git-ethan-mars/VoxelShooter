using Data;
using Entities;
using Infrastructure;
using Infrastructure.Factory;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using Networking.ServerServices;

namespace Networking.MessageHandlers.RequestHandlers
{
    public class TntSpawnHandler : RequestHandler<TntSpawnRequest>
    {
        private readonly IServer _server;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly AudioService _audioService;
        private readonly IEntityFactory _entityFactory;

        public TntSpawnHandler(IServer server, CustomNetworkManager networkManager, AudioService audioService)
        {
            _server = server;
            _coroutineRunner = networkManager;
            _audioService = audioService;
            _entityFactory = networkManager.EntityFactory;
        }

        protected override void OnRequestReceived(NetworkConnectionToClient connection, TntSpawnRequest request)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result || !playerData.IsAlive || playerData.SelectedItem is not TntItem tntItem)
            {
                return;
            }

            var tntData = (TntItemData) playerData.SelectedItemData;
            if (tntData.Amount <= 0)
            {
                return;
            }

            tntData.Amount -= 1;
            connection.Send(new ItemUseResponse(playerData.SelectedSlotIndex, tntData.Amount));
            var tnt = _entityFactory.CreateTnt(request.Position, request.Rotation, _server, connection, tntItem,
                _audioService);
            NetworkServer.Spawn(tnt.gameObject);
            _coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(() => Explode(tnt), tntItem.delayInSeconds));
        }

        private void Explode(Tnt tnt)
        {
            if (tnt != null)
            {
                tnt.Explode();
            }
        }
    }
}