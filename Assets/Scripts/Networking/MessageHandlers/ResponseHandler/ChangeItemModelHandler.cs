using Infrastructure.Factory;
using Infrastructure.Services.StaticData;
using Networking.Messages.Responses;
using PlayerLogic;
using UnityEngine;

namespace Networking.MessageHandlers.ResponseHandler
{
    public class ChangeItemModelHandler : ResponseHandler<ChangeItemModelResponse>
    {
        private readonly IMeshFactory _meshFactory;
        private readonly IStaticDataService _staticData;

        public ChangeItemModelHandler(IMeshFactory meshFactory, IStaticDataService staticData)
        {
            _meshFactory = meshFactory;
            _staticData = staticData;
        }

        protected override void OnResponseReceived(ChangeItemModelResponse response)
        {
            var identity = response.Identity;
            if (identity == null)
            {
                return;
            }

            if (!identity.TryGetComponent<Player>(out var player))
            {
                return;
            }

            if (player.ItemPosition.childCount > 0)
            {
                var item = player.ItemPosition.GetChild(0).gameObject;
                Object.Destroy(item);
            }

            _meshFactory.CreateGameModel(_staticData.GetItem(response.ItemId).prefab,
                identity.GetComponent<Player>().ItemPosition);
        }
    }
}