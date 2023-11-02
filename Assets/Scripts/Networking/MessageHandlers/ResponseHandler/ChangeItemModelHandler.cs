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
            if (response.Identity == null)
            {
                return;
            }

            if (!response.Identity.TryGetComponent<Player>(out var player))
            {
                return;
            }

            foreach (Transform item in player.ItemPosition)
            {
                Object.Destroy(item.gameObject);
            }

            _meshFactory.CreateGameModel(_staticData.GetItem(response.ItemId).prefab,
                player.ItemPosition);
        }
    }
}