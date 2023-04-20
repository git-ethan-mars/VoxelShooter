using Data;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ChangeClassMenu : MonoBehaviour
    {
        [SerializeField] private Button builderButton;
        [SerializeField] private Button sniperButton;
        [SerializeField] private Button combatantButton;
        [SerializeField] private Button grenadierButton;
        private CustomNetworkManager _networkManager;


        public void Construct(CustomNetworkManager networkManager)
        {
            _networkManager = networkManager;
        }
    
        private void Start()
        {
            builderButton.onClick.AddListener(()=>ChangeClass(GameClass.Builder));
            sniperButton.onClick.AddListener(()=>ChangeClass(GameClass.Sniper));
            combatantButton.onClick.AddListener(()=>ChangeClass(GameClass.Combatant));
            grenadierButton.onClick.AddListener(()=>ChangeClass(GameClass.Grenadier));
        }

        private void ChangeClass(GameClass gameClass)
        {
            _networkManager.ChangeClass(gameClass);
            gameObject.SetActive(false);
        }
    }
}
