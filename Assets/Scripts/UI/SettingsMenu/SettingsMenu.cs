using System;
using Infrastructure.Services.Storage;
using UI.Carousel;
using UI.SettingsMenu.States;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.SettingsMenu
{
    public class SettingsMenu : MonoBehaviour
    {
        [Header("Mouse")]
        [SerializeField]
        private Button mouseSectionButton;

        [SerializeField]
        private GameObject mouseSection;

        [SerializeField]
        private SliderWithDisplayedValue generalSensitivity;

        [SerializeField]
        private SliderWithDisplayedValue aimSensitivity;

        [Header("Volume")]
        [SerializeField]
        private Button volumeSectionButton;

        [SerializeField]
        private GameObject volumeSection;

        [SerializeField]
        private SliderWithDisplayedValue masterVolume;

        [SerializeField]
        private SliderWithDisplayedValue musicVolume;

        [SerializeField]
        private SliderWithDisplayedValue soundVolume;

        [Header("Video")]
        [SerializeField]
        private Button videoSectionButton;

        [SerializeField]
        private GameObject videoSection;

        [SerializeField]
        private CarouselControl resolution;

        [SerializeField]
        private CarouselControl screenMode;

        [SerializeField]
        private Button backButton;

        private SettingsMenuStateMachine _menuStateMachine;
        private Action _onBackButtonPressed;

        public void Construct(IStorageService storageService, Action onBackButtonPressed)
        {
            _onBackButtonPressed = onBackButtonPressed;
            _menuStateMachine =
                new SettingsMenuStateMachine(new MouseSettingsState(mouseSection,
                    generalSensitivity, aimSensitivity, storageService), new VolumeSettingsState(volumeSection,
                    masterVolume, musicVolume, soundVolume, storageService), new VideoSettingsState(videoSection,
                    resolution, screenMode, storageService));
            mouseSectionButton.onClick.AddListener(_menuStateMachine.SwitchState<MouseSettingsState>);
            volumeSectionButton.onClick.AddListener(_menuStateMachine.SwitchState<VolumeSettingsState>);
            videoSectionButton.onClick.AddListener(_menuStateMachine.SwitchState<VideoSettingsState>);
            backButton.onClick.AddListener(new UnityAction(_onBackButtonPressed));
            _menuStateMachine.SwitchState<MouseSettingsState>();
        }

        private void OnEnable()
        {
            _menuStateMachine?.SwitchState<MouseSettingsState>();
        }

        private void OnDisable()
        {
            _menuStateMachine?.Reset();
        }

        private void OnDestroy()
        {
            mouseSectionButton.onClick.RemoveListener(_menuStateMachine.SwitchState<MouseSettingsState>);
            volumeSectionButton.onClick.RemoveListener(_menuStateMachine.SwitchState<VolumeSettingsState>);
            videoSectionButton.onClick.RemoveListener(_menuStateMachine.SwitchState<VideoSettingsState>);
            backButton.onClick.RemoveListener(new UnityAction(_onBackButtonPressed));
        }
    }
}