using Infrastructure.Services.Input;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Hud : MonoBehaviour
    {
        public GameObject inventory;
        public Palette palette;
        public GameObject ammoInfo;
        public Image ammoType;
        public TextMeshProUGUI ammoCount;
        public GameObject itemInfo;
        public Image itemIcon;
        public TextMeshProUGUI itemCount;
        public HealthCounter healthCounter;
        private IInputService _inputService;
        private CanvasGroup _canvasGroup;
        
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Update()
        {
            _canvasGroup.alpha = _inputService.IsScoreboardButtonHold() ? 0 : 1;
        }
    }
}
