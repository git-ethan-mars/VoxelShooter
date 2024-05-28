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
        const uint BufferSize = 1024;


        public INetworkedClientState LatestServerState => _messenger.LatestServerState;
        public uint CurrentTick => _currentTick;

        INetworkedClientMessenger<TClientInput, TClientState> _messenger;
        private TClientState _lastProcessedState;

        private readonly TClientInput[] _inputBuffer = new TClientInput[BufferSize];

        private readonly Queue<TClientInput> _inputQueue = new(6);

        private float _minTimeBetweenUpdates;
        private float _timeSinceLastTick;
        private uint _lastProcessedInputTick;
        private uint _currentTick;

        void Awake()
        {
            _messenger = GetComponent<INetworkedClientMessenger<TClientInput, TClientState>>();

            if (_messenger == null)
                Debug.LogError($"Couldn't find sender for {name}");
            else
            {
                _messenger.OnInputReceived += HandleInputReceived;
            }

            _minTimeBetweenUpdates = NetworkServer.sendInterval;
        }

        protected virtual void Update()
        {
            _timeSinceLastTick += Time.deltaTime;

            if (_timeSinceLastTick >= _minTimeBetweenUpdates)
                HandleTick();
        }

        protected abstract void SetState(TClientState state);
        protected abstract void ProcessInput(TClientInput input);
        protected abstract TClientInput GetInput(float deltaTime, uint currentTick);

        private void SendClientInput(TClientInput input)
        {
            _messenger.SendInput(input);
        }

        protected abstract TClientState RecordState(uint lastProcessedInputTick);

        void HandleInputReceived(TClientInput input)
        {
            _inputQueue.Enqueue(input);
        }

        void HandleTick()
        {
            if (isClient && isOwned)
            {

                // Client-side prediction
                if (!isServer && _messenger.LatestServerState != null && (_lastProcessedState == null ||
                                                                          !_lastProcessedState.Equals(_messenger
                                                                              .LatestServerState)))
                {
                    UpdatePrediction(_currentTick, _messenger.LatestServerState);
                }

                var input = GetInput(_timeSinceLastTick, _currentTick);

                var bufferIndex = _currentTick % BufferSize;

                _inputBuffer[bufferIndex] = input;

                SendClientInput(input);

                if (!isServer)
                {
                    ProcessInput(input);
                }
            }
            else if (!isServer)
            {
                HandleOtherPlayerState(_messenger.LatestServerState); // Entity interpolation *TODO
            }

            if (isServer)
            {
                ServerProcessInputsAndSendState();
            }

            _currentTick++;
            _timeSinceLastTick = 0f;
        }

        void UpdatePrediction(uint currentTick, TClientState latestServerState)
        {
            _lastProcessedState = latestServerState;

            SetState(_lastProcessedState);
            
            var index = _lastProcessedState.LastProcessedInputTick + 1;
            while (index < currentTick)
            {
                var input = _inputBuffer[index % BufferSize];
                ProcessInput(input);
                index++;
            }
        }

        protected virtual void HandleOtherPlayerState(TClientState state)
        {
            SetState(state);
        }

        void ServerProcessInputsAndSendState()
        {
            ProcessInputs();
            SendState();
        }

        void ProcessInputs()
        {
            while (_inputQueue.Count > 0)
            {
                var input = _inputQueue.Dequeue();
                ProcessInput(input);

                _lastProcessedInputTick = input.Tick;
            }
        }

        void SendState()
        {
            var state = RecordState(_lastProcessedInputTick);
            _messenger.SendState(state);
        }

        protected void LogState()
        {
            Debug.Log(LatestServerState.ToString());
        }

        protected void LogInputQueue()
        {
            var log = $"Input queue count: {_inputQueue.Count.ToString()}\n";

            for (var i = 0; i < _inputQueue.Count; i++)
            {
                var input = _inputQueue.ElementAt(i);
                log += $"{input.ToString()}\n";
            }

            Debug.Log(log);
        }
    }
}