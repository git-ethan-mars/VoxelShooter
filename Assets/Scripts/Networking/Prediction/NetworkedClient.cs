using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;

namespace Networking.Prediction
{
    public abstract class NetworkedClient<TClientInput, TClientState> : NetworkBehaviour, INetworkedClient
        where TClientInput : INetworkedClientInput
        where TClientState : INetworkedClientState
    {
        private const uint BufferSize = 1024;

        public INetworkedClientState LatestServerState => _messenger.LatestServerState;
        public uint CurrentTick => _currentTick;

        INetworkedClientMessenger<TClientInput, TClientState> _messenger;

        private readonly TClientInput[] _inputBuffer = new TClientInput[BufferSize];
        private readonly TClientState[] _stateBuffer = new TClientState[BufferSize];

        private readonly Queue<TClientInput> _inputQueue = new(6);

        private float _timer;
        private uint _currentTick;

        private void Awake()
        {
            _messenger = GetComponent<INetworkedClientMessenger<TClientInput, TClientState>>();

            if (_messenger == null){
                Debug.LogError($"Couldn't find messenger for {name}");
            }
            else
            {
                _messenger.OnInputReceived += HandleInputReceived;
            }
        }

        protected virtual void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= NetworkServer.sendInterval)
            {
                HandleTick();
                _currentTick++;
                _timer -= NetworkServer.sendInterval;
            }
        }

        protected abstract void SetState(TClientState state);
        protected abstract void ProcessInput(TClientInput input);
        protected abstract TClientInput GetInput(float deltaTime, uint currentTick);

        protected abstract TClientState RecordState(uint lastProcessedInputTick);

        protected virtual void HandleOtherPlayerState(TClientState state)
        {
            SetState(state);
        }

        private void SendClientInput(TClientInput input)
        {
            _messenger.SendInput(input);
        }

        private void HandleInputReceived(TClientInput input)
        {
            _inputQueue.Enqueue(input);
        }

        private void HandleTick()
        {
            if (isClient && isOwned)
            {
                var bufferIndex = _currentTick % BufferSize;
                // Client-side prediction
                if (!isServer && !_stateBuffer[_messenger.LatestServerState.LastProcessedInputTick % BufferSize]
                        .Equals(_messenger.LatestServerState))
                {
                    UpdatePrediction(_currentTick, _messenger.LatestServerState);
                }

                var input = GetInput(_timer, _currentTick);
                _inputBuffer[bufferIndex] = input;

                SendClientInput(input);

                if (!isServer)
                {
                    ProcessInput(input);
                }

                _stateBuffer[bufferIndex] = RecordState(_currentTick);
            }
            else if (!isServer)
            {
                HandleOtherPlayerState(_messenger.LatestServerState); // Entity interpolation *TODO
            }

            if (isServer)
            {
                ServerProcessInputsAndSendState();
            }
        }

        private void UpdatePrediction(uint currentTick, TClientState latestServerState)
        {
            SetState(latestServerState);
            var index = latestServerState.LastProcessedInputTick + 1;
            while (index < currentTick)
            {
                var input = _inputBuffer[index % BufferSize];
                ProcessInput(input);
                index++;
            }
        }

        private void ServerProcessInputsAndSendState()
        {
            if (_inputQueue.Count == 0)
            {
                return;
            }

            uint lastProcessedInputTick = 0;
            while (_inputQueue.Count > 0)
            {
                var input = _inputQueue.Dequeue();
                ProcessInput(input);

                lastProcessedInputTick = input.Tick;
            }

            var state = RecordState(lastProcessedInputTick);
            _messenger.SendState(state);
        }
    }
}