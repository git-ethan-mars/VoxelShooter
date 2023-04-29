using Data;
using Mirror;
using Networking.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChooseClassMenu : MonoBehaviour
    {
        [SerializeField] private Button builderButton;
        [SerializeField] private Button sniperButton;
        [SerializeField] private Button combatantButton;
        [SerializeField] private Button grenadierButton;
        
        
        
        private void Start()
        {
            builderButton.onClick.AddListener(()=>ChangeClass(GameClass.Builder));
            sniperButton.onClick.AddListener(()=>ChangeClass(GameClass.Sniper));
            combatantButton.onClick.AddListener(()=>ChangeClass(GameClass.Combatant));
            grenadierButton.onClick.AddListener(()=>ChangeClass(GameClass.Grenadier));
        }

        private void ChangeClass(GameClass gameClass)
        {
            NetworkClient.Send(new ChangeClassRequest(gameClass));
            gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            Cursor.lockState = CursorLockMode.None;
        }

        private void OnDisable()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
