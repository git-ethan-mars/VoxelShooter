using System;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay;
using R3;
using Services;
using UI;
using UnityEngine;
using VoxelMap;
namespace Infrastructure.States
{
	public class GameLoopState : IPayloadedState<GameSession>
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly IStaticDataService _staticData;
		private readonly IStorageService _storageService;
		private readonly UIProvider _uiProvider;
		private readonly IUIFactory _uiFactory;
		private readonly CameraProvider _cameraProvider;

		private GameSession _gameSession;
		private LoadingWindow _loadingWindow;

		public GameLoopState(GameStateMachine gameStateMachine, IStaticDataService staticData, IStorageService storageService,
			UIProvider uiProvider, IUIFactory uiFactory, CameraProvider cameraProvider)
		{
			_gameStateMachine = gameStateMachine;
			_staticData = staticData;
			_storageService = storageService;
			_uiProvider = uiProvider;
			_uiFactory = uiFactory;
			_cameraProvider = cameraProvider;
		}

		public async void Enter(GameSession gameSession)
		{
			_gameSession = gameSession;
			_loadingWindow = _uiFactory.CreateLoadingWindow();
			_uiProvider.InGameUI = _uiFactory.CreateInGameUI();
			_gameSession.OnMapReady.Subscribe(OnMapReady)
				.AddTo(_gameSession.GameSessionFinished);
			_gameSession.Progress = new Progress<float>(_loadingWindow.UpdateLoadingBar);

			var gameSessionPresenter = new GameSessionPresenter(_gameSession, _loadingWindow, _uiProvider.InGameUI);
			gameSessionPresenter.Initialize();
			
			await _gameSession.RunAsync();
			
			_uiProvider.InGameUI.InGameMenu.ExitButtonPressed
				.Subscribe(_ => OnExitButtonPressed())
				.AddTo(_uiProvider.InGameUI);
			
			
			_storageService.Subscribe<MouseSettingsData>(OnMouseSettingsChanged)
				.AddTo(_uiProvider.InGameUI);
		}

		private void OnMapReady(Map map)
		{
			var mapCenter = new Vector3Ushort((ushort)(map.Width / 2), (ushort)(map.Height / 2), (ushort)(map.Depth / 2));
			_cameraProvider.MainCamera.transform.SetPositionAndRotation(mapCenter, Quaternion.Euler(90, 0, 0));
		}

		public void Exit()
		{
			_gameSession.Dispose();
		}

		private void OnMouseSettingsChanged(MouseSettingsData mouseSettingsData)
		{
			CrosshairSprite crosshairSprite = _staticData.GetCrosshairSprite(mouseSettingsData.CrosshairId);
			_uiProvider.InGameUI.Hud.SetCrosshairIcon(crosshairSprite.Sprite);
		}

		private void OnExitButtonPressed()
		{
			_gameStateMachine.Enter<GameMenuState>();
		}
	}
}