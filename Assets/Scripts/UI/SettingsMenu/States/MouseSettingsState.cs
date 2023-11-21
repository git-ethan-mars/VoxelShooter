using System.Linq;
using Data;
using Infrastructure.Services.Storage;
using UI.Carousel;
using UnityEngine;

namespace UI.SettingsMenu.States
{
    public class MouseSettingsState : ISettingsMenuState
    {
        private readonly GameObject _mouseSection;
        private readonly IStorageService _storageService;
        private readonly CarouselModel<int> _generalSensitivityModel;
        private readonly GeneralSensitivityCarouselView _generalSensitivityView;
        private readonly CarouselModel<int> _aimSensitivityModel;
        private readonly GeneralSensitivityCarouselView _aimSensitivityView;

        public MouseSettingsState(GameObject mouseSection, CarouselControl sensitivityControl,
            CarouselControl aimSensitivityControl, IStorageService storageService)
        {
            _mouseSection = mouseSection;
            _storageService = storageService;
            _generalSensitivityModel = new CarouselModel<int>(Enumerable.Range(0, 101).ToList());
            _generalSensitivityView = new GeneralSensitivityCarouselView(sensitivityControl);
            _aimSensitivityModel = new CarouselModel<int>(Enumerable.Range(0, 101).ToList());
            _aimSensitivityView = new GeneralSensitivityCarouselView(aimSensitivityControl);
        }

        public void Enter()
        {
            _mouseSection.SetActive(true);
            _generalSensitivityModel.ValueChanged += _generalSensitivityView.OnModelValueChanged;
            _generalSensitivityView.IncreaseButtonPressed += _generalSensitivityModel.MoveForward;
            _generalSensitivityView.DecreaseButtonPressed += _generalSensitivityModel.MoveBack;
            _aimSensitivityModel.ValueChanged += _aimSensitivityView.OnModelValueChanged;
            _aimSensitivityView.IncreaseButtonPressed += _aimSensitivityModel.MoveForward;
            _aimSensitivityView.DecreaseButtonPressed += _aimSensitivityModel.MoveBack;

            var currentSettings = _storageService.Load<MouseSettingsData>(Constants.MouseSettingKey);
            _generalSensitivityModel.CurrentItem = currentSettings.GeneralSensitivity;
            _aimSensitivityModel.CurrentItem = currentSettings.AimSensitivity;
        }

        public void Exit()
        {
            _mouseSection.SetActive(false);
            _generalSensitivityModel.ValueChanged -= _generalSensitivityView.OnModelValueChanged;
            _generalSensitivityView.IncreaseButtonPressed -= _generalSensitivityModel.MoveForward;
            _generalSensitivityView.DecreaseButtonPressed -= _generalSensitivityModel.MoveBack;
            _aimSensitivityModel.ValueChanged -= _aimSensitivityView.OnModelValueChanged;
            _aimSensitivityView.IncreaseButtonPressed -= _aimSensitivityModel.MoveForward;
            _aimSensitivityView.DecreaseButtonPressed -= _aimSensitivityModel.MoveBack;

            _storageService.Save(Constants.MouseSettingKey,
                new MouseSettingsData(_generalSensitivityModel.CurrentItem, _aimSensitivityModel.CurrentItem));
        }
    }
}