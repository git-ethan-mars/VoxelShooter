using System;
using GamePlay.Services;
using UI.Carousel;
using UI.SettingsMenuStates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class SettingsMenu : MonoBehaviour
    {
        public event Action BackMousePressed
        {
            add => backButton.onClick.AddListener(new UnityAction(value));
            remove => backButton.onClick.RemoveListener(new UnityAction(value));
        }
        
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

        public void Construct(IStorageService storageService)
        {
            _menuStateMachine =
                new SettingsMenuStateMachine(new MouseSettingsState(mouseSection,
                    generalSensitivity, aimSensitivity, storageService), new VolumeSettingsState(volumeSection,
                    masterVolume, musicVolume, soundVolume, storageService), new VideoSettingsState(videoSection,
                    resolution, screenMode, storageService));
            mouseSectionButton.onClick.AddListener(_menuStateMachine.SwitchState<MouseSettingsState>);
            volumeSectionButton.onClick.AddListener(_menuStateMachine.SwitchState<VolumeSettingsState>);
            videoSectionButton.onClick.AddListener(_menuStateMachine.SwitchState<VideoSettingsState>);
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
        }
    }
}