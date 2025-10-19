using R3;
using Reflex.Attributes;
using Services;
using UI.Carousel;
using UI.SettingsMenuStates;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class SettingsMenu : MonoBehaviour, IBaseMenu
	{
		[Header("Mouse")]
		[SerializeField] private Toggle mouseSectionToggle;
		[SerializeField] private GameObject mouseSection;
		[SerializeField] private SliderWithDisplayedValue generalSensitivity;
		[SerializeField] private SliderWithDisplayedValue aimSensitivity;
		[SerializeField] private CrosshairCarouselView crosshairView;

		[Header("Volume")]
		[SerializeField] private Toggle volumeSectionToggle;
		[SerializeField] private GameObject volumeSection;
		[SerializeField] private SliderWithDisplayedValue masterVolume;
		[SerializeField] private SliderWithDisplayedValue musicVolume;
		[SerializeField] private SliderWithDisplayedValue soundVolume;

		[Header("Video")]
		[SerializeField] private Toggle videoSectionToggle;
		[SerializeField] private GameObject videoSection;
		[SerializeField] private ResolutionCarouselView resolutionView;
		[SerializeField] private ScreenModeCarouselView screenModeView;

		[SerializeField] private Button backButton;
		[SerializeField] private Color activeToggleColor;
		[SerializeField] private Color inactiveToggleColor;

		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }

		private SettingsMenuStateMachine _settingsMenuStateMachine;
		private IStorageService _storageService;
		public Observable<Unit> BackButtonPressed => backButton.onClick.AsObservable();

		[Inject]
		private void Construct(IStorageService storageService, IStaticDataService staticData)
		{
			_storageService = storageService;
			_settingsMenuStateMachine =
				new SettingsMenuStateMachine(new MouseSettingsState(storageService, staticData,
						mouseSection, generalSensitivity, aimSensitivity, crosshairView),
					new VolumeSettingsState(storageService, volumeSection, masterVolume, musicVolume, soundVolume),
					new VideoSettingsState(storageService, videoSection, resolutionView, screenModeView));
			mouseSectionToggle.onValueChanged.AsObservable().Subscribe(OnMouseSectionToggleChanged).AddTo(this);
			volumeSectionToggle.onValueChanged.AsObservable().Subscribe(OnVolumeSectionToggleChanged).AddTo(this);
			videoSectionToggle.onValueChanged.AsObservable().Subscribe(OnVideoSectionToggleChanged).AddTo(this);
		}

		public void Show()
		{
			EventSystem.current.SetSelectedGameObject(mouseSectionToggle.gameObject);
			mouseSectionToggle.SetIsOnWithoutNotify(true);
			OnMouseSectionToggleChanged(true);
		}

		public void Hide()
		{
			_storageService.Save<MouseSettingsData>(IStorageService.MouseSettingsKey);
			_storageService.Save<VolumeSettingsData>(IStorageService.VolumeSettingsKey);
			_storageService.Save<VideoSettingsData>(IStorageService.VideoSettingsKey);
		}

		private void OnDestroy()
		{
			_settingsMenuStateMachine.Reset();
		}

		private void OnMouseSectionToggleChanged(bool value)
		{
			SetToggleColor(mouseSectionToggle, value);

			if (value)
			{
				_settingsMenuStateMachine.SwitchState<MouseSettingsState>();
			}
		}

		private void OnVolumeSectionToggleChanged(bool value)
		{
			SetToggleColor(volumeSectionToggle, value);

			if (value)
			{
				_settingsMenuStateMachine.SwitchState<VolumeSettingsState>();
			}
		}

		private void OnVideoSectionToggleChanged(bool value)
		{
			SetToggleColor(videoSectionToggle, value);

			if (value)
			{
				_settingsMenuStateMachine.SwitchState<VideoSettingsState>();
			}
		}

		private void SetToggleColor(Toggle toggle, bool isOn)
		{
			toggle.targetGraphic.color = isOn ? activeToggleColor : inactiveToggleColor;
		}
	}
}