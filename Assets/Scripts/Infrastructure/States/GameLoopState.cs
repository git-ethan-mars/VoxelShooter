using Common;
using Common.AssetManagement;
using Cysharp.Threading.Tasks;
using GamePlay;
using GamePlay.Audio;
using GamePlay.Services;
using Networking.Client;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using VoxelMap;

namespace Infrastructure.States
{
	public class GameLoopState : IPayloadedState<(IClient client, MapProvider mapProvider)>
	{
		private const float FinalStatisticDuration = 10f;

		private readonly GameStateMachine _gameStateMachine;
		private readonly IStaticDataService _staticData;
		private readonly IAssetProvider _assets;
		private readonly IUIFactory _uiFactory;
		private readonly IInputService _inputService;
		private readonly IStorageService _storageService;
		private readonly IAvatarLoader _avatarLoader;
		private InGameUI _inGameUi;
		private IClient _client;
		private Character _character;
		private Hud _hud;

		public GameLoopState(GameStateMachine gameStateMachine, IStaticDataService staticData, IAssetProvider assets,
			IUIFactory uiFactory,
			IInputService inputService,
			IStorageService storageService,
			IAvatarLoader avatarLoader)
		{
			_gameStateMachine = gameStateMachine;
			_staticData = staticData;
			_assets = assets;
			_uiFactory = uiFactory;
			_inputService = inputService;
			_storageService = storageService;
			_avatarLoader = avatarLoader;
		}

		public void Enter((IClient client, MapProvider mapProvider) payload)
		{
			AudioPlayer.Initialize(_assets);
			_client = payload.client;
			_inGameUi = _uiFactory.CreateInGameUI(_inputService, _storageService, _avatarLoader);
			_client.ScoreboardChanged += _inGameUi.Scoreboard.UpdateScoreboard;
			_client.GameTimeChanged += _inGameUi.TimeCounter.ChangeGameTime;
			_client.RespawnTimeChanged += _inGameUi.TimeCounter.ChangeRespawnTime;
			_client.GameFinished += OnGameFinished;
			_client.CharacterSpawned += OnCharacterSpawned;
			_client.CharacterDespawned += OnCharacterDespawned;
			_inGameUi.ChooseClassMenu.ChangeClassButtonPressed += _client.ChangeClass;
			_inGameUi.InGameMenu.ExitButtonPressed += OnExitButtonPressed;
			_hud = _uiFactory.CreateHud();
			_hud.gameObject.SetActive(false);
			_hud.InventoryPresenter.Construct(_inputService);
			_hud.PalettePresenter.Construct(_staticData, _inputService);
			_hud.PalettePresenter.Initialize();
		}
		
		private void OnCharacterSpawned(Character character)
		{
			_character = character;
			_character.HealthSystem.HealthChanged += _hud.HealthCounter.SetHealthValue;
			_hud.gameObject.SetActive(true);
			_hud.PalettePresenter.gameObject.SetActive(false);
			_hud.InventoryPresenter.Initialize(character.Inventory);
			_hud.InventoryPresenter.gameObject.SetActive(true);
		}

		private void OnCharacterDespawned(Character character)
		{
			_character.HealthSystem.HealthChanged -= _hud.HealthCounter.SetHealthValue;
			_hud.InventoryPresenter.gameObject.SetActive(false);
			_hud.InventoryPresenter.ResetInventory();
		}

		private async UniTask OnGameFinished()
		{
			_inGameUi.ShowFinalStatistic();
			await UniTask.WaitForSeconds(FinalStatisticDuration);
			_gameStateMachine.Enter<MainMenuState>();
		}

		private void OnExitButtonPressed()
		{
			_client.Stop();
			_gameStateMachine.Enter<MainMenuState>();
		}

		public void Exit()
		{
			_client.ScoreboardChanged -= _inGameUi.Scoreboard.UpdateScoreboard;
			_client.GameTimeChanged -= _inGameUi.TimeCounter.ChangeGameTime;
			_client.RespawnTimeChanged -= _inGameUi.TimeCounter.ChangeRespawnTime;
			_client.GameFinished -= OnGameFinished;
			_client.CharacterSpawned -= OnCharacterSpawned;
			_client.CharacterDespawned -= OnCharacterDespawned;
			_inGameUi.ChooseClassMenu.ChangeClassButtonPressed -= _client.ChangeClass;
			_inGameUi.InGameMenu.ExitButtonPressed -= OnExitButtonPressed;
			Object.Destroy(_inGameUi.gameObject);
		}
	}
}