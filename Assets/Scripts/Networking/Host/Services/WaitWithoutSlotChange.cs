using Mirror;
using UnityEngine;

namespace Networking.Host.Services
{
    public class WaitWithoutSlotChange : CustomYieldInstruction
    {
        public bool CompletedSuccessfully { get; private set; }
        public bool IsAborted { get; private set; }
        private readonly IHost _host;
        private readonly NetworkConnectionToClient _connection;
        private readonly int _slotIndex;
        private readonly float _waitingTime;
        private float _elapsedTime;

        public override bool keepWaiting
        {
            get
            {
                var result = _host.TryGetPlayerData(_connection, out var playerData);
                if (!result || !playerData.IsAlive)
                {
                    IsAborted = true;
                    CompletedSuccessfully = false;
                    return false;
                }

                if (playerData.SelectedSlotIndex != _slotIndex)
                {
                    CompletedSuccessfully = false;
                    return false;
                }

                _elapsedTime += Time.deltaTime;
                if (_elapsedTime >= _waitingTime)
                {
                    CompletedSuccessfully = true;
                    return false;
                }

                return true;
            }
        }

        public WaitWithoutSlotChange(IHost host, NetworkConnectionToClient connection, float waitingTime)
        {
            _host = host;
            _connection = connection;
            _waitingTime = waitingTime;
            _slotIndex = _host.GetPlayerData(connection).SelectedSlotIndex;
        }
    }
}