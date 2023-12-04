using System.Collections.Generic;
using System.Linq;
using Data;
using Infrastructure.Services.Storage;
using UI.Carousel;
using UnityEngine;

namespace UI.SettingsMenu.States
{
    public class VideoSettingsState : ISettingsMenuState
    {
        private readonly GameObject _videoSection;
        private readonly IStorageService _storageService;
        private readonly CarouselModel<Resolution> _resolutionModel;
        private readonly ResolutionCarouselView _resolutionView;
        private readonly CarouselModel<FullScreenMode> _screenModeModel;
        private readonly ScreenModeCarouselView _screenModeView;

        public VideoSettingsState(GameObject videoSection, CarouselControl resolutionControl,
            CarouselControl screenModeControl, IStorageService storageService)
        {
            _videoSection = videoSection;
            _storageService = storageService;
            _resolutionModel = new CarouselModel<Resolution>(Screen.resolutions.ToList());
            _resolutionView = new ResolutionCarouselView(resolutionControl);
            _screenModeModel =
                new CarouselModel<FullScreenMode>(new List<FullScreenMode>
                {
                    FullScreenMode.FullScreenWindow, FullScreenMode.Windowed
                });
            _screenModeView = new ScreenModeCarouselView(screenModeControl);
        }

        public void Enter()
        {
            _videoSection.SetActive(true);
            _resolutionModel.ValueChanged += _resolutionView.OnModelValueChanged;
            _resolutionView.IncreaseButtonPressed += _resolutionModel.MoveForward;
            _resolutionView.DecreaseButtonPressed += _resolutionModel.MoveBack;
            _screenModeModel.ValueChanged += _screenModeView.OnModelValueChanged;
            _screenModeView.IncreaseButtonPressed += _screenModeModel.MoveForward;
            _screenModeView.DecreaseButtonPressed += _screenModeModel.MoveBack;

            var currentSettings = _storageService.Load<VideoSettingsData>(Constants.VideoSettingsKey);
            _resolutionModel.CurrentItem = currentSettings.Resolution;
            _screenModeModel.CurrentItem = currentSettings.ScreenMode;
        }

        public void Exit()
        {
            _videoSection.SetActive(false);
            _resolutionModel.ValueChanged -= _resolutionView.OnModelValueChanged;
            _resolutionView.IncreaseButtonPressed -= _resolutionModel.MoveForward;
            _resolutionView.DecreaseButtonPressed -= _resolutionModel.MoveBack;
            _screenModeModel.ValueChanged -= _screenModeView.OnModelValueChanged;
            _screenModeView.IncreaseButtonPressed -= _screenModeModel.MoveForward;
            _screenModeView.DecreaseButtonPressed -= _screenModeModel.MoveBack;

            _storageService.Save(Constants.VideoSettingsKey,
                new VideoSettingsData(_resolutionModel.CurrentItem, _screenModeModel.CurrentItem));
        }
    }
}