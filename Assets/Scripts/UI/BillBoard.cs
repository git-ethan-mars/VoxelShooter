using PlayerLogic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class BillBoard : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nickNameText;
        [SerializeField] private Player player;
        private Transform _cameraTransform;
        private void Start()
        {
            nickNameText.SetText(player.nickName);
            _cameraTransform = Camera.main.transform;
        
        }

        // Update is called once per frame
        private void LateUpdate()
        {
            transform.LookAt(transform.position + _cameraTransform.rotation * Vector3.forward, _cameraTransform.rotation * Vector3.up);
        }
    }
}
