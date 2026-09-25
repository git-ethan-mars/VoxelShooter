using System;
using Cysharp.Threading.Tasks;
using GamePlay;
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
		private readonly FallMeshGenerator _fallMeshGenerator;

		private CompositeDisposable _disposables;

		public GameLoopState(GameStateMachine gameStateMachine, IUIFactory uiFactory,
			MapProvider mapProvider, VSNetworkManager networkManager, AudioPlayer audioPlayer, FallMeshGenerator fallMeshGenerator)
		{
			_gameStateMachine = gameStateMachine;
			_uiFactory = uiFactory;
			_mapProvider = mapProvider;
			_networkManager = networkManager;
			_audioPlayer = audioPlayer;
			_fallMeshGenerator = fallMeshGenerator;
		}

		public UniTask EnterAsync(GameMode gameMode)
		{
			LoadingWindow loadingWindow = _uiFactory.CreateLoadingWindow();
			GameModeView gameModeView = _uiFactory.CreateGameModeView(gameMode);
			_mapProvider.MapLoadingProgress = new Progress<float>(loadingWindow.UpdateLoadingBar);

			_disposables = new CompositeDisposable();

			Observable.EveryUpdate()
				.Subscribe(_ => gameMode.Update())
				.AddTo(_disposables);

			_networkManager.MessageReceived.OfMessageType<FallingVoxelsResponse>()
				.Subscribe(directedMessage => _fallMeshGenerator.GenerateFallVoxels(directedMessage.Message.Voxels))
				.AddTo(_disposables);
			_networkManager.MessageReceived.OfMessageType<StaticAudioResponse>()
				.Subscribe(directedMessage => _audioPlayer.PlayAsync(directedMessage.Message.AudioType, directedMessage.Message.Position).Forget())
				.AddTo(_disposables);
			_networkManager.MessageReceived.OfMessageType<DynamicAudioResponse>()
				.Subscribe(directedMessage => _audioPlayer.PlayAsync(directedMessage.Message.AudioType, directedMessage.Message.NetworkIdentity,
					directedMessage.Message.IsSpatial).Forget())
				.AddTo(_disposables);

			gameModeView.InGameMenu.ExitButtonPressed
				.Subscribe(_ => OnExitButtonPressed().Forget())
				.AddTo(_disposables);
			gameMode.GameState.Subscribe(gameModeView.OnGameStateChanged)
				.AddTo(_disposables);
			_mapProvider.Map.Where(map => map)
				.Subscribe(gameModeView.OnMapChanged)
				.AddTo(_disposables);

			return UniTask.CompletedTask;
		}

		public void Exit()
		{
			_disposables.Dispose();

			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				_networkManager.StopHost();
			}
			else
			{
				_networkManager.StopClient();
			}
		}

		private async UniTaskVoid OnExitButtonPressed()
		{
			await _gameStateMachine.EnterAsync<GameMenuState>();
		}
	}
}
