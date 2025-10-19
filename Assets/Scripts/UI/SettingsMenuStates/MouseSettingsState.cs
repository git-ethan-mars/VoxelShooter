using System;
using System.Linq;
using Data;
using R3;
using Services;
using UI.Carousel;
using UnityEngine;
namespace UI.SettingsMenuStates
{
	public class MouseSettingsState : ISettingsMenuState
	{
		private const float MinSliderValue = 1.0f;
		private const float MaxSliderValue = 5.0f;
		private readonly SliderWithDisplayedValue _aimSensitivity;
		private readonly CarouselModel<CrosshairSprite> _crosshairModel;
		private readonly CarouselPresenter<CrosshairSprite> _crosshairPresenter;

		private readonly GameObject _mouseSection;
		private readonly SliderWithDisplayedValue _sensitivity;
		private readonly IStorageService _storageService;

		private IDisposable _saveSettingsSubscription;

		public MouseSettingsState(IStorageService storageService, IStaticDataService staticData, GameObject mouseSection,
			SliderWithDisplayedValue sensitivity, SliderWithDisplayedValue aimSensitivity, CrosshairCarouselView crosshairView)
		{
			_storageService = storageService;
			_mouseSection = mouseSection;
			_sensitivity = sensitivity;
			_aimSensitivity = aimSensitivity;
			var currentSettings = _storageService.Load<MouseSettingsData>(IStorageService.MouseSettingsKey);
			_sensitivity.Construct(currentSettings.GeneralSensitivity, MinSliderValue, MaxSliderValue);
			_aimSensitivity.Construct(currentSettings.AimSensitivity, MinSliderValue, MaxSliderValue);
			_crosshairModel = new CarouselModel<CrosshairSprite>(staticData.GetCrosshairSprite(currentSettings.CrosshairId),
				staticData.GetCrosshairSprites().ToArray());
			_crosshairPresenter = new CarouselPresenter<CrosshairSprite>(_crosshairModel, crosshairView);
		}

		public void Enter()
		{
			_mouseSection.SetActive(true);

			_crosshairPresenter.Initialize();
			_saveSettingsSubscription = Observable.Merge(_sensitivity.Slider.Select(_ => Unit.Default), _aimSensitivity.Slider.Select(_ => Unit
				.Default), _crosshairModel.CurrentItem.Select(_ => Unit.Default)).Subscribe(_ =>
				_storageService.Set(new MouseSettingsData(_sensitivity.Slider.CurrentValue, _aimSensitivity.Slider.CurrentValue,
						_crosshairModel.CurrentItem.CurrentValue.ID)));
		}

		public void Exit()
		{
			_crosshairPresenter.Dispose();
			_saveSettingsSubscription.Dispose();

			_mouseSection.SetActive(false);
		}
	}
}