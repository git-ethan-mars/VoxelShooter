using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class Player : NetworkBehaviour
    {
        public float PlaceDistance { get; set; }
        public int BlockCount { get; set; }
        public Transform itemPosition;
        private GameObject _hud;

        public void Construct(PlayerCharacteristic characteristic)
        {
            PlaceDistance = characteristic.placeDistance;
            BlockCount = characteristic.blockCount;
        }
        
        public override void OnStartLocalPlayer()
        {
            _hud = AllServices.Container.Single<IGameFactory>().CreateHud(gameObject);
        }

        public void OnDestroy()
        {
            Destroy(_hud);
        }
    }
}
