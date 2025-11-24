using System;
using Cysharp.Threading.Tasks;
using Data;
using GamePlay;
using R3;
namespace UI
{
	public class GameSessionPresenter
	{
		private readonly GameSession _gameSession;
		private readonly LoadingWindow _loadingWindow;
		private readonly InGameUI _inGameUI;

		public GameSessionPresenter(GameSession gameSession, LoadingWindow loadingWindow, InGameUI inGameUI)
		{
			_gameSession = gameSession;
			_loadingWindow = loadingWindow;
			_inGameUI = inGameUI;
		}

		public void Initialize()
		{
			_gameSession.State
				.Subscribe(OnGameSessionStateChanged)
				.AddTo(_gameSession.GameSessionFinished);
			_gameSession.GameTime
				.Subscribe(OnGameTimeChanged)
				.AddTo(_gameSession.GameSessionFinished);
			_gameSession.RespawnTime
				.Subscribe(OnRespawnTimeChanged)
				.AddTo(_gameSession.GameSessionFinished);
			_inGameUI.ChooseClassMenu.ChangeClassButtonPressed
				.Subscribe(OnChangeClassButtonPressed)
				.AddTo(_gameSession.GameSessionFinished);
		}

		private void OnGameSessionStateChanged(GameSessionState state)
		{
			if (state == GameSessionState.Playing)
			{
				_inGameUI.Show();
				_loadingWindow.Hide();
			}
			else if (state == GameSessionState.Waiting)
			{
				_loadingWindow.Show();
				_inGameUI.Hide();
			}
		}

		private void OnGameTimeChanged(TimeSpan timeLeft)
		{
			_inGameUI.TimeInfo.ChangeGameTime(timeLeft);
		}

		private void OnRespawnTimeChanged(TimeSpan timeLeft)
		{
			_inGameUI.TimeInfo.ChangeRespawnTime(timeLeft);
		}

		private async void OnChangeClassButtonPressed(GameClass chosenClass)
		{
			var gameClass = await _gameSession.ChangeGameClassAsync(chosenClass);
		}
	}
}