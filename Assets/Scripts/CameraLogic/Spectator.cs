using Infrastructure.Services;
using Infrastructure.Services.Input;
using Mirror;
using Networking.Messages;
using UnityEngine;

namespace CameraLogic
{
    public class Spectator : NetworkBehaviour
    {
        [SerializeField] private float sensitivityX;
        [SerializeField] private float sensitivityY;
        [SerializeField] private float distance;
        private IInputService _inputService;
        private NetworkIdentity _target;
        private int _targetConnectionId;
        private Vector3 _staticMapPosition;
        private float XRotation { get; set; }
        private float YRotation { get; set; }

        public void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
        }

        public void Start()
        {
            if (!isLocalPlayer)
                return;
            var cameraTransform = Camera.main.transform;
            cameraTransform.parent = transform;
            cameraTransform.position = Vector3.zero;
            cameraTransform.rotation = Quaternion.identity;
            NetworkClient.RegisterHandler<SpectatorTargetResult>(OnCameraTargetReceived);
            NetworkClient.Send(new KillerCameraRequest());
            enabled = false;
        }

        private void OnCameraTargetReceived(SpectatorTargetResult message)
        {
            _target = message.NewTarget;
            if (_target is null)
            {
                _staticMapPosition = message.Position;
            }
            else
            {
                _targetConnectionId = _target.connectionToClient.connectionId;
            }

            if (!enabled)
                enabled = true;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (!IsValidCameraPosition)
            {
                NetworkClient.Send(new NextPlayerCameraRequest(_targetConnectionId));
                enabled = false;
                return;
            }

            if (_inputService.IsFirstActionButtonDown())
            {
                NetworkClient.Send(new NextPlayerCameraRequest(_targetConnectionId));
            }

            var mouseXInput = _inputService.GetMouseHorizontalAxis();
            var mouseYInput = _inputService.GetMouseVerticalAxis();
            var mouseX = mouseXInput * sensitivityX * Time.deltaTime;
            var mouseY = mouseYInput * sensitivityY * Time.deltaTime;
            YRotation += mouseX;
            XRotation -= mouseY;
            transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
            transform.position = _target is not null
                ? _target.transform.position - distance * transform.forward
                : _staticMapPosition - distance * transform.forward;
        }

        private bool IsValidCameraPosition => _target is not null || _staticMapPosition != Vector3.zero;

        private void OnDestroy()
        {
            NetworkClient.UnregisterHandler<SpectatorTargetResult>();
        }
    }
}