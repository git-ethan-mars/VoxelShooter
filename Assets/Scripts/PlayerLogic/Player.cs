using System.Collections.Generic;
using Infrastructure.Factory;
using Infrastructure.Services;
using Mirror;
using Networking;
using UnityEngine;

namespace PlayerLogic
{
    public class Player : NetworkBehaviour
    {
        public List<int> ItemsIds { get; private set; }
        public float PlaceDistance { get; private set; }

        public string nickName;

        [SerializeField]
        private Transform itemPosition;

        public Transform ItemPosition => itemPosition;

        [SerializeField]
        private GameObject[] bodyParts;

        [SerializeField]
        private GameObject nickNameCanvas;

        private GameObject _hud;
        private IClient _client;

        public void Construct(IClient client, float placeDistance, List<int> itemIds)
        {
            _client = client;
            PlaceDistance = placeDistance;
            nickName = "Need to fix";
            ItemsIds = itemIds;
        }

        public override void OnStartLocalPlayer()
        {
            _hud = AllServices.Container.Single<IUIFactory>().CreateHud(_client, gameObject);
            TurnOffNickName();
            TurnOffBodyRender();
        }

        private void TurnOffBodyRender()
        {
            for (var i = 0; i < bodyParts.Length; i++)
            {
                bodyParts[i].GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void TurnOffNickName()
        {
            nickNameCanvas.SetActive(false);
        }

        private void OnDestroy()
        {
            Destroy(_hud);
        }
    }
}