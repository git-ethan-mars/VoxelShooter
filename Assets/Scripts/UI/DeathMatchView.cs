using System;
using GamePlay;
using R3;
using Reflex.Attributes;
using Services;
using UI.InGameUIStates;
using UnityEngine;

namespace UI
{
	public class DeathMatchView : GameModeView
	{
		[SerializeField] private TimeInfo timeInfo;
		[SerializeField] private ChooseClassMenu chooseClassMenu;
		[SerializeField] private DeathMatchScoreboardView deathMatchScoreboard;
		[SerializeField] private VotingView votingView;

		private TimeSpan _respawnTimer;

		private UIProvider _uiProvider;
		private DeathMatch _deathMatch;

		[Inject]
		private void Construct(IInputService inputService, IStaticDataService staticData, IStorageService storageService,
			CharacterProvider characterProvider, UIProvider uiProvider)
		{
			base.Construct(inputService, staticData, storageService, characterProvider);
			_uiProvider = uiProvider;
		}

		public void Initialize(DeathMatch deathMatch)
		{
			_deathMatch = deathMatch;

			chooseClassMenu.Initialize();
			chooseClassMenu.ChangeClassButtonPressed.Subscribe(_deathMatch.ChangeClass).AddTo(this);
			chooseClassMenu.ChangeClassButtonPressed.Subscribe(_ => SwitchState<DefaultState>()).AddTo(this);
			chooseClassMenu.ExitButtonPressed.Subscribe(_ => SwitchState<DefaultState>()).AddTo(this);

			_deathMatch.CharacterDied.Subscribe(_ => OnCharacterDied()).AddTo(this);
			_deathMatch.TimeLeft.Subscribe(OnGameTimeChanged).AddTo(this);

			votingView.Initialize(_deathMatch.MapVoting);

			deathMatchScoreboard.Initialize(_deathMatch.Scoreboard);

			var scoreBoardState = new ScoreboardState(deathMatchScoreboard);
			AddState(scoreBoardState);

			var chooseClassMenuState = new ChooseClassMenuState(InputService, chooseClassMenu);
			AddState(chooseClassMenuState);

			var deathMatchState = new DeathMatchState(CharacterProvider, timeInfo, Hud, inventoryView);
			AddState<DefaultState>(deathMatchState);

			SwitchState<ChooseClassMenuState>();
		}

		public override void OnGameStateChanged(GameState gameState)
		{
			if (gameState == GameState.Loading)
			{
				_uiProvider.LoadingWindow.Show();
			}

			if (gameState == GameState.Playing)
			{
				_uiProvider.LoadingWindow.Hide();
			}

			if (gameState == GameState.ShowingStatistics)
			{
			}
		}

		protected override void Update()
		{
			base.Update();

			if (InputService.IsChooseClassButtonDown())
			{
				SwitchState<ChooseClassMenuState>();
			}

			if (InputService.IsScoreboardButtonDown())
			{
				SwitchState<ScoreboardState>();
			}

			if (InputService.IsScoreboardButtonUp())
			{
				SwitchState<DefaultState>();
			}

			if (_respawnTimer == TimeSpan.Zero)
			{
				return;
			}

			_respawnTimer -= TimeSpan.FromSeconds(Time.deltaTime);

			if (_respawnTimer < TimeSpan.Zero)
			{
				_respawnTimer = TimeSpan.Zero;
			}

			timeInfo.ChangeRespawnTime(TimeSpan.FromSeconds(_respawnTimer.TotalSeconds));
		}

		private void OnGameTimeChanged(TimeSpan timeLeft)
		{
			timeInfo.ChangeGameTime(timeLeft);
		}

		private void OnCharacterDied()
		{
			_respawnTimer = _deathMatch.GameSettings.RespawnTime;
		}
	}
}
