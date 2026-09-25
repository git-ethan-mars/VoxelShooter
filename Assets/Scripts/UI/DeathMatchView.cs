using System;
using GamePlay;
using R3;
using Reflex.Attributes;
using Services;
using UI.InGameUIStates;
using UnityEngine;
using AudioType = Data.AudioType;

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
		private AudioPlayer _audioPlayer;
		private DeathMatch _deathMatch;

		[Inject]
		private void Construct(IInputService inputService, IStaticDataService staticData, IStorageService storageService,
			CharacterProvider characterProvider, UIProvider uiProvider, AudioPlayer audioPlayer)
		{
			base.Construct(inputService, staticData, storageService, characterProvider);
			_uiProvider = uiProvider;
			_audioPlayer = audioPlayer;
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
			_deathMatch.MapVoting.Started.Subscribe(_ => _audioPlayer.PlayAsync(AudioType.MapVoting).Forget()).AddTo(this);

			deathMatchScoreboard.Initialize(_deathMatch.Scoreboard);

			var scoreBoardState = new ScoreboardState(deathMatchScoreboard);
			AddState(scoreBoardState);

			var chooseClassMenuState = new ChooseClassMenuState(InputService, chooseClassMenu);
			AddState(chooseClassMenuState);

			var deathMatchState = new DeathMatchState(CharacterProvider, timeInfo, Hud, inventoryView);
			AddState<DefaultState>(deathMatchState);

			SwitchState<ChooseClassMenuState>();
		}

		protected override void Update()
		{
			base.Update();

			if (InputService.IsChooseClassButtonDown() && IsInState<DefaultState>())
			{
				SwitchState<ChooseClassMenuState>();
			}

			if (InputService.IsScoreboardButtonDown() && IsInState<DefaultState>())
			{
				SwitchState<ScoreboardState>();
			}

			if (InputService.IsScoreboardButtonUp() && IsInState<ScoreboardState>())
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

		public override void OnGameStateChanged(GameState gameState)
		{
			if (gameState == GameState.Loading)
			{
				_uiProvider.LoadingWindow.Show();
			}

			if (gameState == GameState.Playing)
			{
				_uiProvider.LoadingWindow.Hide();
				SwitchState<ChooseClassMenuState>();
			}

			if (gameState == GameState.ShowingStatistics)
			{
			}
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
