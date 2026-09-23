using System;
using GamePlay;
using GamePlay.Audio;
using Mirror;
using Networking;
using Networking.Core;
using Networking.Messages;
using R3;
using UI;
using VoxelMap;

namespace Infrastructure.States
{
    public class GameLoopState : IPayloadedState<GameMode>
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly IUIFactory _uiFactory;
        private readonly MapProvider _mapProvider;
        private readonly VSNetworkManager _networkManager;
        private readonly AudioPlayer _audioPlayer;

        private IDisposable _updateLoop;

        public GameLoopState(GameStateMachine gameStateMachine, IUIFactory uiFactory,
            MapProvider mapProvider, VSNetworkManager networkManager, AudioPlayer audioPlayer)
        {
            _gameStateMachine = gameStateMachine;
            _uiFactory = uiFactory;
            _mapProvider = mapProvider;
            _networkManager = networkManager;
            _audioPlayer = audioPlayer;
        }

        public void Enter(GameMode gameMode)
        {
            LoadingWindow loadingWindow = _uiFactory.CreateLoadingWindow();
            GameModeView gameModeView = _uiFactory.CreateGameModeView(gameMode);
            _mapProvider.MapLoadingProgress = new Progress<float>(loadingWindow.UpdateLoadingBar);
            
            _updateLoop = Observable.EveryUpdate().Subscribe(_ => gameMode.Update());

            _networkManager.MessageReceived.OfMessageType<StaticAudioResponse>()
                .Subscribe(directedMessage => _audioPlayer.Play(directedMessage.Message.AudioType, directedMessage.Message.Position).Forget())
                .AddTo(_networkManager);
            _networkManager.MessageReceived.OfMessageType<DynamicAudioResponse>()
                .Subscribe(directedMessage => _audioPlayer.Play(directedMessage.Message.AudioType, directedMessage.Message.NetworkIdentity,
                    directedMessage.Message.IsSpatial).Forget())
                .AddTo(_networkManager);

            gameModeView.InGameMenu.ExitButtonPressed
                .Subscribe(_ => OnExitButtonPressed())
                .AddTo(gameModeView);
            gameMode.GameState.Subscribe(gameModeView.OnGameStateChanged)
                .AddTo(gameModeView);
            _mapProvider.Map.Where(map => map).Subscribe(gameModeView.OnMapChanged);
        }

        public void Exit()
        {
            _updateLoop.Dispose();

            if (_networkManager.mode == NetworkManagerMode.Host)
            {
                _networkManager.StopHost();
            }
            else
            {
                _networkManager.StopClient();
            }
        }

        private void OnExitButtonPressed()
        {
            _gameStateMachine.Enter<GameMenuState>();
        }
    }
}