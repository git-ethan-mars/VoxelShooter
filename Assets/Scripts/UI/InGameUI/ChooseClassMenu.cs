using Data;
using Mirror;
using Networking.Messages.Requests;
using UnityEngine;
using UnityEngine.UI;

namespace UI.InGameUI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ChooseClassMenu : MonoBehaviour
    {
        [SerializeField]
        private CanvasGroup canvasGroup;

        public CanvasGroup CanvasGroup => canvasGroup;

        [SerializeField]
        private Button builderButton;

        public Button BuilderButton => builderButton;

        [SerializeField]
        private Button sniperButton;

        public Button SniperButton => sniperButton;

        [SerializeField]
        private Button combatantButton;

        public Button CombatantButton => combatantButton;

        [SerializeField]
        private Button grenadierButton;

        public Button GrenadierButton => grenadierButton;

        [SerializeField]
        private Button exitButton;

        public Button ExitButton => exitButton;

        public void Construct()
        {
            builderButton.onClick.AddListener(ChooseBuilder);
            sniperButton.onClick.AddListener(ChooseSniper);
            combatantButton.onClick.AddListener(ChooseCombatant);
            grenadierButton.onClick.AddListener(ChooseGrenadier);
            canvasGroup.alpha = 0.0f;
        }

        private void ChooseBuilder()
        {
            ChangeClass(GameClass.Builder);
        }

        private void ChooseSniper()
        {
            ChangeClass(GameClass.Sniper);
        }

        private void ChooseCombatant()
        {
            ChangeClass(GameClass.Combatant);
        }

        private void ChooseGrenadier()
        {
            ChangeClass(GameClass.Grenadier);
        }

        private void ChangeClass(GameClass gameClass)
        {
            NetworkClient.Send(new ChangeClassRequest(gameClass));
        }

        private void OnDestroy()
        {
            builderButton.onClick.RemoveListener(ChooseBuilder);
            sniperButton.onClick.RemoveListener(ChooseSniper);
            combatantButton.onClick.RemoveListener(ChooseCombatant);
            grenadierButton.onClick.RemoveListener(ChooseGrenadier);
        }
    }
}