using Data;
using Entities;
using Infrastructure;
using Infrastructure.Factory;
using Inventory.Tnt;
using Mirror;
using Networking.Messages.Requests;
using Networking.Messages.Responses;
using Networking.ServerServices;
using UnityEngine;

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
            var playerFound = _server.TryGetPlayerData(connection, out var playerData);
            if (!playerFound || !playerData.IsAlive || playerData.SelectedItem is not TntItem tntItem)
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
            var raycastResult = Physics.Raycast(request.Ray, out var raycastHit,
                playerData.Characteristic.placeDistance, Constants.buildMask);
            if (!raycastResult)
            {
                return;
            }

            var tntPosition = Vector3Int.FloorToInt(raycastHit.point + raycastHit.normal / 2) +
                              TntPlaceHelper.GetTntOffsetPosition(raycastHit.normal);
            var tntRotation = TntPlaceHelper.GetTntRotation(raycastHit.normal);
            var linkedPosition = Vector3Int.FloorToInt(raycastHit.point - raycastHit.normal / 2);
            var tnt = _entityFactory.CreateTnt(tntPosition, tntRotation, tntItem,
                _server, connection, _audioService, linkedPosition);
            _server.EntityContainer.AddExplosive(tnt);
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