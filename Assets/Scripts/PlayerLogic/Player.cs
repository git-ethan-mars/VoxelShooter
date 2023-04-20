using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class Player : NetworkBehaviour
    {
        [HideInInspector] [SyncVar] public float placeDistance;
        [HideInInspector] [SyncVar] public int blockCount;
        public Transform itemPosition;
        private GameObject _hud;

        public void Construct(PlayerCharacteristic characteristic)
        {
            placeDistance = characteristic.placeDistance;
            blockCount = characteristic.blockCount;
        }
        
        public override void OnStartLocalPlayer()
        {
            _hud = AllServices.Container.Single<IGameFactory>().CreateHud(gameObject);
            Debug.Log("HUD created");
        }

        public void OnDestroy()
        {
            Destroy(_hud);
            Debug.Log("HUD DESTROYED");
        }
    }
}
