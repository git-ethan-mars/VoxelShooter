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
        [SerializeField] private GameObject nickNameCanvas;
        [Header("Body parts")] 
        [SerializeField] private Transform head;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform chest;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;
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
            TurnOffBodyRender();
            TurnOffNickName();
        }

        private void UpdateHealth(int oldHealth, int newHealth)
        {
            OnHealthChanged?.Invoke(newHealth);
        }

        public void OnDestroy()
        {
            Destroy(_hud);
        }

        private void TurnOffBodyRender()
        {
            head.gameObject.GetComponent<Renderer>().enabled = false;
            leftArm.gameObject.GetComponent<Renderer>().enabled = false;
            rightArm.gameObject.GetComponent<Renderer>().enabled = false;
            chest.gameObject.GetComponent<Renderer>().enabled = false;
            leftLeg.gameObject.GetComponent<Renderer>().enabled = false;
            rightLeg.gameObject.GetComponent<Renderer>().enabled = false;
        }

        private void TurnOffNickName()
        {
            nickNameCanvas.SetActive(false);
        }
    }
}