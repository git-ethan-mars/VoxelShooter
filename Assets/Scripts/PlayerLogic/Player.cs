using System;
using Data;
using Infrastructure.Factory;
using Infrastructure.Services;
using Mirror;
using UnityEngine;

namespace PlayerLogic
{
    public class Player : NetworkBehaviour
    {
        public event Action<int> OnHealthChanged;
        [HideInInspector] [SyncVar] public float placeDistance;
        [HideInInspector] [SyncVar] public string nickName;
        [SyncVar(hook = nameof(UpdateHealth))] [HideInInspector] public int health;
        public Transform itemPosition;
        [SerializeField] private GameObject[] bodyParts;
        [SerializeField] private GameObject nickNameCanvas;
        private GameObject _hud;

        public void Construct(PlayerData playerData)
        {
            placeDistance = playerData.Characteristic.placeDistance;
            nickName = playerData.NickName;
            health = playerData.Characteristic.maxHealth;
        }

        public override void OnStartLocalPlayer()
        {
            _hud = AllServices.Container.Single<IUIFactory>().CreateHud(gameObject);
            TurnOffNickName();
            TurnOffBodyRender();
        }

        private void UpdateHealth(int oldHealth, int newHealth)
        {
            OnHealthChanged?.Invoke(newHealth);
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