using System;
using R3;
using Services;
using UI.Carousel;
using UnityEngine;
namespace UI.SettingsMenuStates
{
	public class VideoSettingsState : ISettingsMenuState
	{
		private readonly CarouselModel<Resolution> _resolutionModel;
		private readonly CarouselPresenter<Resolution> _resolutionPresenter;
		private readonly CarouselModel<FullScreenMode> _screenModeModel;
		private readonly CarouselPresenter<FullScreenMode> _screenModePresenter;
		private readonly IStorageService _storageService;
		private readonly GameObject _videoSection;

		private IDisposable _disposable;

		public VideoSettingsState(IStorageService storageService, GameObject videoSection, ResolutionCarouselView resolutionView,
			ScreenModeCarouselView screenModeView)
		{
			_videoSection = videoSection;
			_storageService = storageService;

			var currentSettings = _storageService.Load<VideoSettingsData>(IStorageService.VideoSettingsKey);
			_resolutionModel = new CarouselModel<Resolution>(currentSettings.Resolution, Screen.resolutions);
			_screenModeModel =
				new CarouselModel<FullScreenMode>(currentSettings.ScreenMode, FullScreenMode.FullScreenWindow, FullScreenMode.Windowed);

			_resolutionPresenter = new CarouselPresenter<Resolution>(_resolutionModel, resolutionView);
			_screenModePresenter = new CarouselPresenter<FullScreenMode>(_screenModeModel, screenModeView);
		}

		public void Enter()
		{
			_videoSection.SetActive(true);

			_resolutionPresenter.Initialize();
			_screenModePresenter.Initialize();

			IDisposable resolutionSubscription = _resolutionModel.CurrentItem
				.Where(resolution => !resolution.Equals(Screen.currentResolution))
				.Subscribe(resolution => Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode));
			IDisposable screenModeSubscription = _screenModeModel.CurrentItem.Subscribe(screenMode => Screen.fullScreenMode = screenMode);
			IDisposable saveSettingsSubscription = Observable.Merge(_resolutionModel.CurrentItem.Select(_ => Unit.Default),
					_screenModeModel.CurrentItem.Select(_ => Unit.Default))
				.Subscribe(_ => _storageService.Set(new VideoSettingsData(_resolutionModel.CurrentItem.Value, _screenModeModel.CurrentItem.Value)));
			_disposable = Disposable.Combine(resolutionSubscription, screenModeSubscription, saveSettingsSubscription, _resolutionPresenter,
				_screenModePresenter);
		}

		public void Exit()
		{
			_videoSection.SetActive(false);
			_disposable.Dispose();
		}
	}
}