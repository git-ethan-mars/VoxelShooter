using Data;
using Infrastructure.Services.Storage;
using UnityEngine;

namespace UI.SettingsMenu.States
{
    public class MouseSettingsState : ISettingsMenuState
    {
        private const float MinSliderValue = 0.0f;
        private const float MaxSliderValue = 1.0f;

        private readonly GameObject _mouseSection;
        private readonly IStorageService _storageService;
        private readonly SliderWithDisplayedValue _sensitivity;
        private readonly SliderWithDisplayedValue _aimSensitivity;

        public MouseSettingsState(GameObject mouseSection, SliderWithDisplayedValue sensitivity,
            SliderWithDisplayedValue aimSensitivity, IStorageService storageService)
        {
            _mouseSection = mouseSection;
            _storageService = storageService;
            _sensitivity = sensitivity;
            _aimSensitivity = aimSensitivity;
            var currentSettings = _storageService.Load<MouseSettingsData>(Constants.MouseSettingsKey);
            _sensitivity.Construct(currentSettings.GeneralSensitivity, MinSliderValue, MaxSliderValue);
            _aimSensitivity.Construct(currentSettings.AimSensitivity, MinSliderValue, MaxSliderValue);
        }

        public void Enter()
        {
            _mouseSection.SetActive(true);
        }

        public void Exit()
        {
            _storageService.Save(Constants.MouseSettingsKey,
                new MouseSettingsData(_sensitivity.SliderValue, _aimSensitivity.SliderValue));
            _mouseSection.SetActive(false);
        }
    }
}