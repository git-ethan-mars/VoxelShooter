using System;
using Data;
using Infrastructure.Services.Input;
using Mirror;
using Networking;
using Networking.Messages;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ChooseClassMenu : Window
    {
        [SerializeField] private Button builderButton;
        [SerializeField] private Button sniperButton;
        [SerializeField] private Button combatantButton;
        [SerializeField] private Button grenadierButton;
        private IInputService _inputService;
        private CanvasGroup _canvasGroup;
        private bool _isLocalBuild;

        public void Construct(CustomNetworkManager networkManager, IInputService inputService, bool isLocalBuild)
        {
            _isLocalBuild = isLocalBuild;
            _inputService = inputService;
            networkManager.GameFinished += () => gameObject.SetActive(false);
            _canvasGroup = GetComponent<CanvasGroup>();
            builderButton.onClick.AddListener(() => ChangeClass(GameClass.Builder));
            sniperButton.onClick.AddListener(() => ChangeClass(GameClass.Sniper));
            combatantButton.onClick.AddListener(() => ChangeClass(GameClass.Combatant));
            grenadierButton.onClick.AddListener(() => ChangeClass(GameClass.Grenadier));
        }

        private void OnEnable()
        {
            ShowCursor();
        }

        private void Update()
        {
            if (_inputService.IsChooseClassButtonDown())
            {
                if (Math.Abs(_canvasGroup.alpha - 1) < Constants.Epsilon)
                {
                    _canvasGroup.alpha = 0;
                    HideCursor();
                }
                else
                {
                    _canvasGroup.alpha = 1;
                    ShowCursor();
                }
            }
        }

        private void OnDisable()
        {
            HideCursor();
        }

        private void ChangeClass(GameClass gameClass)
        {
            NetworkClient.Send(new ChangeClassRequest(_isLocalBuild ? CSteamID.Nil : SteamUser.GetSteamID(),
                gameClass, _isLocalBuild ? Random.value.ToString() : SteamFriends.GetPersonaName()));
            _canvasGroup.alpha = 0;
            HideCursor();
        }
    }
}