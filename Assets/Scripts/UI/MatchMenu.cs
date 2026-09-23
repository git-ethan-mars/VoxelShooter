using System.IO;
using System.Linq;
using System.Threading;
using Data;
using R3;
using Reflex.Attributes;
using Services;
using TMPro;
using UI.Carousel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VoxelMap;
namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class MatchMenu : BaseMenu
	{
		[SerializeField] private Button backButton;
		[SerializeField] private Button resetButton;
		[SerializeField] private Button applyButton;

		[Header("Game Duration")]
		[SerializeField] private TextMeshProUGUI gameDuration;
		[SerializeField] private Button incrementGameDuration;
		[SerializeField] private Button decrementGameDuration;

		[Header("Map choice")]
		[SerializeField] private MapCarouselView mapCarouselView;
		
		private LobbyBalance _lobbyBalance;
		private IMapConfigureLoader _mapConfigureLoader;

		private Limitation _timeLimitation;

		private CarouselPresenter<MapView> _mapCarouselPresenter;
		private CarouselModel<MapView> _mapCarouselModel;
		private CancellationTokenSource _cts;

		[Inject]
		private void Construct(IMapConfigureLoader mapConfigureLoader, IStaticDataService staticData)
		{
			_mapConfigureLoader = mapConfigureLoader;
			_lobbyBalance = staticData.GetLobbyBalance();

			resetButton.OnClickAsObservable().Subscribe(_ => OnResetButton()).AddTo(this);

			mapCarouselView.IncreaseButtonPressed.Subscribe(_ => OnNextMapButtonPressed()).AddTo(this);
			mapCarouselView.DecreaseButtonPressed.Subscribe(_ => OnPreviousMapButtonPressed()).AddTo(this);

			InitGameDuration();
			InitMapChoice();
		}

		private async void OnNextMapButtonPressed()
		{
			await mapCarouselView.PlayMapImageAnimationAsync(false, _cts.Token);
		}

		private async void OnPreviousMapButtonPressed()
		{
			await mapCarouselView.PlayMapImageAnimationAsync(true, _cts.Token);
		}

		private void OnDestroy()
		{
			_mapCarouselPresenter.Dispose();
		}

		public Observable<Unit> BackButtonPressed => backButton.onClick.AsObservable();
		public Observable<GameSettings> ApplyButtonPressed => applyButton.onClick.AsObservable().Select(_ => GetWorldSettings());

		public override void Show()
		{
			_cts = new CancellationTokenSource().AddTo(this);
			EventSystem.current.SetSelectedGameObject(applyButton.gameObject);
		}

		public override void Hide()
		{
			_cts.Cancel();
			_cts.Dispose();
		}

		private void InitMapChoice()
		{
			var maps = LoadMaps();
			_mapCarouselModel = new CarouselModel<MapView>(maps[0], maps);
			_mapCarouselPresenter = new CarouselPresenter<MapView>(_mapCarouselModel, mapCarouselView);
			_mapCarouselPresenter.Initialize();
		}

		private void InitGameDuration()
		{
			_timeLimitation = new Limitation(_lobbyBalance.minMatchDuration, _lobbyBalance.maxMatchDuration);
			_timeLimitation.Subscribe(value => gameDuration.SetText(value.ToString())).AddTo(this);
			incrementGameDuration.OnClickAsObservable().Subscribe(_ => _timeLimitation.Value++).AddTo(this);
			decrementGameDuration.OnClickAsObservable().Subscribe(_ => _timeLimitation.Value--).AddTo(this);
		}

		private void OnResetButton()
		{
			_timeLimitation.Reset();
		}

		private MapView[] LoadMaps()
		{
			if (!Directory.Exists(Constants.MapFolderPath))
			{
				Directory.CreateDirectory(Constants.MapFolderPath);
			}

			var mapNames = MapDataReader.GetExistedMaps()
				.Select(fileName => new MapView(fileName, _mapConfigureLoader.GetMapConfigure(fileName).Image))
				.ToArray();
			return mapNames;
		}

		private GameSettings GetWorldSettings()
		{
			string mapName = _mapCarouselModel.CurrentItem.CurrentValue.MapName;
			return new GameSettings(mapName, _timeLimitation.Value, _lobbyBalance.spawnTime, _lobbyBalance.spawnTime);
		}
	}
}