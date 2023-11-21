using System;
using Data;
using Infrastructure.Services.Input;
using Mirror;
using Networking;
using Networking.Messages.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ChooseClassMenu : Window
    {
        [SerializeField]
        private CanvasGroup canvasGroup;
        
        [SerializeField]
        private Button builderButton;

        [SerializeField]
        private Button sniperButton;

        [SerializeField]
        private Button combatantButton;

        [SerializeField]
        private Button grenadierButton;
        
        [SerializeField]
        private Button exitButton;

        private IInputService _inputService;
        private IClient _client;

        public void Construct(IClient client, IInputService inputService)
        {
            _inputService = inputService;
            _client = client;
            _client.GameFinished += HideWindow;
            builderButton.onClick.AddListener(() => ChangeClass(GameClass.Builder)); // TODO : Unsubscribe from these events.
            sniperButton.onClick.AddListener(() => ChangeClass(GameClass.Sniper));
            combatantButton.onClick.AddListener(() => ChangeClass(GameClass.Combatant));
            grenadierButton.onClick.AddListener(() => ChangeClass(GameClass.Grenadier));
            exitButton.onClick.AddListener(() => canvasGroup.alpha = 0);
        }

        private void HideWindow()
        {
            _client.GameFinished -= HideWindow;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            ShowCursor();
        }

        private void Update()
        {
            if (_inputService.IsChooseClassButtonDown())
            {
                if (Math.Abs(canvasGroup.alpha - 1) < Constants.Epsilon)
                {
                    canvasGroup.alpha = 0;
                    HideCursor();
                }
                else
                {
                    canvasGroup.alpha = 1;
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
            NetworkClient.Send(new ChangeClassRequest(gameClass));
            canvasGroup.alpha = 0;
            HideCursor();
        }
    }
}