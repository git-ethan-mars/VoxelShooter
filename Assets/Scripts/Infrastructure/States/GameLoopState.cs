using Common;
using Common.AssetManagement;
using Common.Input;
using Common.Services.StaticData;
using Common.StaticData;
using Common.Storage;
using Infrastructure.Factory;
using Infrastructure.Services.PlayerDataLoader;
using MapLogic;
using Networking.Client;
using UI;
using UnityEngine;
using VoxelMap;

namespace Infrastructure.States
{
	public class GameLoopState : IPayloadedState<(IClient client, string mapName, MapData mapData)>
	{
		private const float FinalStatisticDuration = 10f;

		private readonly GameStateMachine _gameStateMachine;
		private readonly ICoroutineRunner _coroutineRunner;
		private readonly IAssetProvider _assets;
		private readonly IUIFactory _uiFactory;
		private readonly IInputService _inputService;
		private readonly IStorageService _storageService;
		private readonly IAvatarLoader _avatarLoader;
		private readonly IStaticDataService _staticData;
		private InGameUI _inGameUi;
		private IClient _client;

		public GameLoopState(GameStateMachine gameStateMachine, ICoroutineRunner coroutineRunner, IAssetProvider assets,
			IStaticDataService staticData,
			IUIFactory uiFactory,
			IInputService inputService,
			IStorageService storageService,
			IAvatarLoader avatarLoader)
		{
			_gameStateMachine = gameStateMachine;
			_coroutineRunner = coroutineRunner;
			_assets = assets;
			_staticData = staticData;
			_uiFactory = uiFactory;
			_inputService = inputService;
			_storageService = storageService;
			_avatarLoader = avatarLoader;
		}

		public void Enter((IClient client, string mapName, MapData mapData) payload)
		{
			_client = payload.client;
			var mapConfigure = _staticData.GetMapConfigure(payload.mapName);
			var map = new MapBuilder(_assets)
				.WithWaterColor(mapConfigure.WaterColor)
				.WithInnerColor(mapConfigure.InnerColor)
				.WithAmbient(mapConfigure.AmbientData)
				.WithDirectionalLight(mapConfigure.LightData)
				.WithFog(mapConfigure.FogData)
				.WithSkybox(mapConfigure.SkyboxMaterial)
				.WithWalls()
				.Build(payload.mapData); 
			_inGameUi = _uiFactory.CreateInGameUI(_inputService, _storageService, _avatarLoader);
			_client.ScoreboardChanged += _inGameUi.Scoreboard.UpdateScoreboard;
			_client.GameTimeChanged += _inGameUi.TimeCounter.ChangeGameTime;
			_client.RespawnTimeChanged += _inGameUi.TimeCounter.ChangeRespawnTime;
			_client.GameFinished += OnGameFinished;
			_inGameUi.ChooseClassMenu.ChangeClassButtonPressed += _client.ChangeClass;
			_inGameUi.InGameMenu.ExitButtonPressed += OnExitButtonPressed;
		}

		private void OnGameFinished()
		{
			_inGameUi.ShowFinalStatistic();
			_coroutineRunner.StartCoroutine(Utils.DoActionAfterDelay(_gameStateMachine.Enter<MainMenuState>,
				FinalStatisticDuration));
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
			Object.Destroy(_inGameUi.gameObject);
		}
	}
}