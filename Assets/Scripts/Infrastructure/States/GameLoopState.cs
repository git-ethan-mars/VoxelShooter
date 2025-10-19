using System;
using Data;
using Mirror;
using Networking;
using Networking.Messages;
using R3;
using Services;
using UI;
using UnityEngine;
namespace Infrastructure.States
{
	public class GameLoopState : IState
	{
		private readonly GameStateMachine _gameStateMachine;
		private readonly IStaticDataService _staticData;
		private readonly IStorageService _storageService;
		private readonly VoxelShooterNetworkManager _networkManager;
		private readonly UIProvider _uiProvider;
		private readonly IUIFactory _uiFactory;

		private IDisposable _disposable;

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

		public void Enter()
		{
			_uiProvider.InGameUI = _uiFactory.CreateInGameUI();
			_uiProvider.InGameUI.InGameMenu.ExitButtonPressed
				.Subscribe(_ => OnExitButtonPressed())
				.AddTo(_uiProvider.InGameUI);
			_uiProvider.InGameUI.ChooseClassMenu.ChangeClassButtonPressed
				.Subscribe(OnChangeClassButtonPressed)
				.AddTo(_uiProvider.InGameUI);
			_disposable = _storageService.Subscribe<MouseSettingsData>(OnMouseSettingsChanged);
		}

		public void Exit()
		{
			_disposable.Dispose();
		}

		private void StartGameTimer(WorldSettings worldSettings)
		{
			TimeSpan gameDuration = TimeSpan.Zero;
			Observable.Interval(TimeSpan.FromSeconds(1), _networkManager.HostStopped)
				.TakeWhile(_ => gameDuration < worldSettings.GameDuration)
				.Subscribe(_ =>
				{
					_uiProvider.InGameUI.TimeInfo.ChangeGameTime(worldSettings.GameDuration - gameDuration);
					gameDuration += TimeSpan.FromSeconds(1);
				}, _ => OnGameFinished());
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
			StopNetwork();
			_gameStateMachine.Enter<GameMenuState>();
		}

		private async void OnGameFinished()
		{
			try
			{
				StopNetwork();
				await _uiProvider.InGameUI.ShowFinalStatisticAsync();
				_gameStateMachine.Enter<GameMenuState>();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		private void StopNetwork()
		{
			if (_networkManager.mode == NetworkManagerMode.Host)
			{
				_networkManager.StopHost();
			}
			else
			{
				_networkManager.StopClient();
			}
		}
	}
}