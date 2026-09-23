using System;
using System.Collections.Generic;
using Data;
using GamePlay;
using R3;
using Services;
using UI.InGameUIStates;
using UI.Inventory;
using UnityEngine;
using VoxelMap;

namespace UI
{
	public abstract class GameModeView : MonoBehaviour
	{
		protected IInputService InputService;
		protected CharacterProvider CharacterProvider;
		[SerializeField] protected InventoryView inventoryView;
		private Dictionary<Type, IInGameUIState> _states;
		private IInGameUIState _currentState;
		private IStaticDataService _staticData;
		private IStorageService _storageService;
		[SerializeField] private CanvasGroup canvasGroup;

		[field: SerializeField] public InGameMenu InGameMenu { get; private set; }
		[field: SerializeField] public SettingsMenu SettingsMenu { get; private set; }
		[field: SerializeField] public Hud Hud { get; private set; }
		[field: SerializeField] public WorldMap WorldMap { get; private set; }

		protected void Construct(IInputService inputService, IStaticDataService staticData,
			IStorageService storageService, CharacterProvider characterProvider)
		{
			InputService = inputService;
			CharacterProvider = characterProvider;
			_staticData = staticData;
			_storageService = storageService;
			_states = new Dictionary<Type, IInGameUIState>
			{
				[typeof(DefaultState)] = new DefaultState(characterProvider, Hud, inventoryView),
				[typeof(InGameMenuState)] = new InGameMenuState(inputService, InGameMenu),
				[typeof(SettingsMenuState)] = new SettingsMenuState(inputService, SettingsMenu),
				[typeof(WorldMapState)] = new WorldMapState(WorldMap)
			};
		}

		public abstract void OnGameStateChanged(GameState gameState);

		public void OnMapChanged(Map map)
		{
			canvasGroup.alpha = 1;
			SettingsMenu.BackButtonPressed
				.Subscribe(_ => SwitchState<InGameMenuState>())
				.AddTo(map);
			InGameMenu.ResumeButtonPressed
				.Subscribe(_ => SwitchState<DefaultState>())
				.AddTo(map);
			InGameMenu.SettingsButtonPressed
				.Subscribe(_ => SwitchState<SettingsMenuState>())
				.AddTo(map);
			_storageService.Subscribe<MouseSettingsData>(OnMouseSettingsChanged)
				.AddTo(map);
			WorldMap.OnMapChanged(map);
		}

		protected virtual void Update()
		{
			if (InputService.IsScoreboardButtonUp())
			{
				SwitchState<DefaultState>();
			}

			if (InputService.IsInGameMenuButtonDown())
			{
				SwitchState<InGameMenuState>();
			}

			if (InputService.IsMapButtonDown())
			{
				SwitchState<WorldMapState>();
			}

			if (InputService.IsMapButtonUp())
			{
				SwitchState<DefaultState>();
			}
		}

		protected void AddState<T>(T state) where T : IInGameUIState
		{
			if (state == null)
			{
				throw new ArgumentNullException(nameof(state));
			}

			_states[typeof(T)] = state;
		}

		protected void SwitchState<T>() where T : IInGameUIState
		{
			if (_currentState is T)
			{
				return;
			}

			_currentState?.Exit();
			_currentState = _states[typeof(T)];
			_currentState.Enter();

			Debug.Log(_currentState.GetType().Name);
		}

		private void OnMouseSettingsChanged(MouseSettingsData mouseSettingsData)
		{
			CrosshairSprite crosshairSprite = _staticData.GetCrosshairSprite(mouseSettingsData.CrosshairId);
			Hud.SetCrosshairIcon(crosshairSprite.Sprite);
		}

		private void OnDestroy()
		{
			_currentState?.Exit();
		}
	}
}
