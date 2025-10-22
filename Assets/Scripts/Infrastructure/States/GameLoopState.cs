using System;
using Data;
using GamePlay;
using Networking;
using Networking.Messages;
using R3;
using Services;
using UI;
using UnityEngine;
namespace Infrastructure.States
{
	public class GameLoopState : IPayloadedState<GameSession>
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly IStaticDataService _staticData;
		private readonly IStorageService _storageService;
		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly UIProvider _uiProvider;
		private readonly IUIFactory _uiFactory;

		private GameSession _gameSession;
		private LoadingWindow _loadingWindow;

		public GameLoopState(GameStateMachine gameStateMachine, IStaticDataService staticData, IStorageService storageService,
			VoxelShooterNetworkManager networkManager, UIProvider uiProvider, IUIFactory uiFactory)
		{
			_gameStateMachine = gameStateMachine;
			_staticData = staticData;
			_storageService = storageService;
			_networkManager = networkManager;
			_uiProvider = uiProvider;
			_uiFactory = uiFactory;
		}

		public async void Enter(GameSession gameSession)
		{
			_gameSession = gameSession;
			_loadingWindow = _uiFactory.CreateLoadingWindow();

			var progress = new Progress<float>(_loadingWindow.UpdateLoadingBar);
			
			await _gameSession.RunAsync(progress);
			
			_uiProvider.InGameUI = _uiFactory.CreateInGameUI();
			
			_gameSession.State
				.Subscribe(OnGameSessionStateChanged)
				.AddTo(_uiProvider.InGameUI);
			_gameSession.TimeLeft
				.Subscribe(_uiProvider.InGameUI.TimeInfo.ChangeGameTime)
				.AddTo(_uiProvider.InGameUI);
			_uiProvider.InGameUI.InGameMenu.ExitButtonPressed
				.Subscribe(_ => OnExitButtonPressed())
				.AddTo(_uiProvider.InGameUI);
			_uiProvider.InGameUI.ChooseClassMenu.ChangeClassButtonPressed
				.Subscribe(OnChangeClassButtonPressed)
				.AddTo(_uiProvider.InGameUI);
			
			_storageService.Subscribe<MouseSettingsData>(OnMouseSettingsChanged)
				.AddTo(_uiProvider.InGameUI);
		}

		public void Exit()
		{
			_gameSession.Dispose();
		}

		private void OnClientDisconnected()
		{
			Debug.Log("Client disconnected");
		}

		private void OnMouseSettingsChanged(MouseSettingsData mouseSettingsData)
		{
			CrosshairSprite crosshairSprite = _staticData.GetCrosshairSprite(mouseSettingsData.CrosshairId);
			_uiProvider.InGameUI.Hud.SetCrosshairIcon(crosshairSprite.Sprite);
		}

		private void OnChangeClassButtonPressed(GameClass chosenClass)
		{
			var request = new ChangeClassRequest(chosenClass);
			_networkManager.SendRequest(request);
		}

		private void OnExitButtonPressed()
		{
			_gameStateMachine.Enter<GameMenuState>();
		}

		private void OnGameSessionStateChanged(GameSessionState state)
		{
			switch (state)
			{
				case GameSessionState.Playing:
					_uiProvider.InGameUI.Show();
					_loadingWindow.Hide();
					break;
				case GameSessionState.Waiting:
					_loadingWindow.Show();
					_uiProvider.InGameUI.Hide();
					break;
			}
		}
	}
}