using System.Collections.Generic;
using Infrastructure.Factory;
using Infrastructure.Services.Input;
using Networking;
using UnityEngine;

namespace PlayerLogic
{
    public class Player : MonoBehaviour
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

        public void Construct(IClient client, IUIFactory uiFactory, IInputService inputService, float placeDistance,
            List<int> itemIds)
        {
            _client = client;
            nickName = "Need to fix";
            PlaceDistance = placeDistance;
            ItemsIds = itemIds;
            _hud = uiFactory.CreateHud(_client, inputService, gameObject);
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