using Infrastructure.Services;
using Infrastructure.Services.Input;
using Mirror;
using Networking;
using UnityEngine;

namespace CameraLogic
{
    public class Spectator : NetworkBehaviour
    {
        [SerializeField] private float sensitivityX;
        [SerializeField] private float sensitivityY;
        [SerializeField] private float distance;
        private IInputService _inputService;
        private Transform _cameraTransform;
        private NetworkIdentity _target;
        private Vector3 _staticMapPosition;
        private bool _messageSent;
        private IServer _server;

        private float XRotation { get; set; }
        private float YRotation { get; set; }

        public void Construct(IServer server)
        {
            _server = server;
        }

        public void Awake()
        {
            _inputService = AllServices.Container.Single<IInputService>();
        }


        public override void OnStartLocalPlayer()
        {
            SetupCamera();
            RequestNextIdentity();
        }

        private void Update()
        {
            if (!isLocalPlayer && !IsValidCameraPosition) return;
            if (_inputService.IsFirstActionButtonDown()) RequestNextIdentity();
            var mouseXInput = _inputService.GetMouseHorizontalAxis();
            var mouseYInput = _inputService.GetMouseVerticalAxis();
            var mouseX = mouseXInput * sensitivityX * Time.deltaTime;
            var mouseY = mouseYInput * sensitivityY * Time.deltaTime;
            YRotation += mouseX;
            XRotation -= mouseY;
            transform.rotation = Quaternion.Euler(XRotation, YRotation, 0);
            if (_target != null)
                transform.position = _target.transform.position - distance * transform.forward;
            else
                transform.position = _staticMapPosition - distance * transform.forward;
        }

        private bool IsValidCameraPosition => _target != null || _staticMapPosition != Vector3.zero;

        private void SetupCamera()
        {
            _cameraTransform = Camera.main.transform;
            _cameraTransform.parent = transform;
            _cameraTransform.position = Vector3.zero;
            _cameraTransform.rotation = Quaternion.identity;
            Camera.main.fieldOfView = Constants.DefaultFov;
        }


        [Command]
        private void RequestNextIdentity(NetworkConnectionToClient connection = null)
        {
            var result = _server.Data.TryGetPlayerData(connection, out var playerData);
            if (!result) return;
            if (playerData.SpectatedPlayer == null)
            {
                for (var i = _server.Data.KillStatistics.Count - 1; i >= 0; i--)
                {
                    var killData = _server.Data.KillStatistics[i];
                    if (killData.Victim != connection) continue;
                    result = _server.Data.TryGetPlayerData(killData.Killer, out var killerData);
                    if (!result) continue;
                    if (!killerData.IsAlive)
                        break;
                    SetCameraTarget(killData.Killer.identity);
                    playerData.SpectatedPlayer = killData.Killer;
                    return;
                }
            }

            var alivePlayers = _server.Data.GetAlivePlayers(connection);
            if (alivePlayers.Count == 0)
            {
                var mapWidth = _server.MapProvider.MapData.Width;
                var mapHeight = _server.MapProvider.MapData.Height;
                var mapDepth = _server.MapProvider.MapData.Depth;
                SetCameraTarget(new Vector3(mapWidth / 2, mapHeight / 2, mapDepth / 2));
                return;
            }

            var index = 0;
            for (; index < alivePlayers.Count; index++)
            {
                if (alivePlayers[index].Key == playerData.SpectatedPlayer)
                {
                    SetCameraTarget(alivePlayers[(index + 1) % alivePlayers.Count].Key.identity);
                    playerData.SpectatedPlayer =
                        alivePlayers[(index + 1) % alivePlayers.Count].Key;
                    return;
                }
            }

            SetCameraTarget(alivePlayers[0].Key.identity);
            playerData.SpectatedPlayer = alivePlayers[0].Key;
        }

        [TargetRpc]
        private void SetCameraTarget(NetworkIdentity identity)
        {
            _target = identity;
        }

        [TargetRpc]
        private void SetCameraTarget(Vector3 position)
        {
            _staticMapPosition = position;
        }
    }
}