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

		private IInputService _inputService;
		private InGameUIStateMachine _uiStateMachine;
		private CompositeDisposable _disposable;

		[field: SerializeField] public TimeInfo TimeInfo { get; private set; }
		[field: SerializeField] public ChooseClassMenu ChooseClassMenu { get; private set; }
		[field: SerializeField] public InGameMenu InGameMenu { get; private set; }
		[field: SerializeField] public ScoreboardView Scoreboard { get; private set; }
		[field: SerializeField] public SettingsMenu SettingsMenu { get; private set; }
		[field: SerializeField] public Hud Hud { get; private set; }
		[field: SerializeField] public WorldMap WorldMap { get; private set; }
		[SerializeField] private CanvasGroup canvasGroup;

		[Inject]
		private void Construct(IInputService inputService, CharacterProvider characterProvider)
		{
			_inputService = inputService;
			_uiStateMachine = new InGameUIStateMachine(inputService, characterProvider, this);
		}
		
		public void Show()
		{
			canvasGroup.alpha = 1;
			Initialize();
		}

		public void Hide()
		{
			canvasGroup.alpha = 0;
			_disposable?.Dispose();
		}

		private void Initialize()
		{
			_uiStateMachine.SwitchState<ChooseClassMenuState>();
			
			_disposable = new CompositeDisposable();
			
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsScoreboardButtonUp())
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(_disposable);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsScoreboardButtonDown())
				.Subscribe(_ => _uiStateMachine.SwitchState<ScoreboardState>())
				.AddTo(_disposable);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsChooseClassButtonDown())
				.Subscribe(_ => _uiStateMachine.SwitchState<ChooseClassMenuState>())
				.AddTo(_disposable);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsInGameMenuButtonDown())
				.Subscribe(_ => _uiStateMachine.SwitchState<InGameMenuState>())
				.AddTo(_disposable);
			Observable.EveryUpdate()
				.Where(_ => _inputService.IsMapButtonUp())
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(_disposable);
			/*Observable.EveryUpdate()
				.Where(_ => _inputService.IsMapButtonDown())
				.Subscribe(_ => _uiStateMachine.SwitchState<WorldMapState>())
				.AddTo(_disposable);*/
			ChooseClassMenu.ChangeClassButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(_disposable);
			ChooseClassMenu.ExitButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(_disposable);
			SettingsMenu.BackButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(_disposable);
			InGameMenu.ResumeButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<DefaultState>())
				.AddTo(_disposable);
			InGameMenu.SettingsButtonPressed
				.Subscribe(_ => _uiStateMachine.SwitchState<SettingsMenuState>())
				.AddTo(_disposable);
		}

		private void OnDestroy()
		{
			_uiStateMachine.Destroy();
			_disposable?.Dispose();
		}
	}
}