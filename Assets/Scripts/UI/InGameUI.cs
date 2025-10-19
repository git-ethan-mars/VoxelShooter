using Cysharp.Threading.Tasks;
using GamePlay;
using R3;
using Reflex.Attributes;
using Services;
using UI.InGameUIStates;
using UnityEngine;
namespace UI
{
	public class InGameUI : MonoBehaviour
	{
		private const float FinalStatisticDuration = 10f;
		
		private IInputService _inputService;
		private InGameUIStateMachine _uiStateMachine;

		[field: SerializeField] public TimeInfo TimeInfo { get; private set; }
		[field: SerializeField] public ChooseClassMenu ChooseClassMenu { get; private set; }
		[field: SerializeField] public InGameMenu InGameMenu { get; private set; }
		[field: SerializeField] public ScoreboardView Scoreboard { get; private set; }
		[field: SerializeField] public SettingsMenu SettingsMenu { get; private set; }
		[field: SerializeField] public Hud Hud { get; private set; }
		[field: SerializeField] public WorldMap WorldMap { get; private set; }

		[Inject]
		private void Construct(IInputService inputService, CharacterProvider characterProvider)
		{
			_inputService = inputService;
			_uiStateMachine = new InGameUIStateMachine(inputService, characterProvider, this);
		}

		public void Initialize()
		{
			_uiStateMachine.SwitchState<ChooseClassMenuState>();
			
			DisposableBuilder disposableBuilder = Disposable.CreateBuilder();
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsScoreboardButtonUp())
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(ref disposableBuilder);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsScoreboardButtonDown())
				.Subscribe(_ => _uiStateMachine.SwitchState<ScoreboardState>())
				.AddTo(ref disposableBuilder);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsChooseClassButtonDown())
				.Subscribe(_ => _uiStateMachine.SwitchState<ChooseClassMenuState>())
				.AddTo(ref disposableBuilder);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsInGameMenuButtonDown())
				.Subscribe(_ => _uiStateMachine.SwitchState<InGameMenuState>())
				.AddTo(ref disposableBuilder);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsMapButtonUp())
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(ref disposableBuilder);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsMapButtonDown())
				.Subscribe(_ => _uiStateMachine.SwitchState<WorldMapState>())
				.AddTo(ref disposableBuilder);
			ChooseClassMenu.ChangeClassButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(ref disposableBuilder);
			ChooseClassMenu.ExitButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(ref disposableBuilder);
			SettingsMenu.BackButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(ref disposableBuilder);
			InGameMenu.ResumeButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(ref disposableBuilder);
			InGameMenu.SettingsButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<SettingsMenuState>())
				.AddTo(ref disposableBuilder);

			disposableBuilder.Build().AddTo(this);
		}

		private void OnDestroy()
		{
			_uiStateMachine.Destroy();
		}

		public async UniTask ShowFinalStatisticAsync()
		{
			enabled = false;
			_uiStateMachine.SwitchState<ScoreboardState>();
			await UniTask.WaitForSeconds(FinalStatisticDuration);
		}
	}
}