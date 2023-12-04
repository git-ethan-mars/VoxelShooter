using Data;
using Infrastructure.Services.StaticData;
using Infrastructure.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MatchMenu : Window
    {
        [SerializeField]
        private Button backButton;

        [SerializeField]
        private Button resetButton;

        [SerializeField]
        private Button applyButton;

        [Header("Game Duration")]
        [SerializeField]
        private TextMeshProUGUI gameDuration;

        [SerializeField]
        private Button incrementGameDuration;

        [SerializeField]
        private Button decrementGameDuration;

        [Header("Map choice")]
        [SerializeField]
        private TextMeshProUGUI mapName;

        [SerializeField]
        private Button nextMapButton;

        [SerializeField]
        private Button previousMapButton;

        [SerializeField]
        private RawImage mapImage;

        private Limitation _timeLimitation;
        private IMapRepository _mapRepository;
        private GameStateMachine _stateMachine;
        private int _minGameTime;
        private int _maxGameTime;
        private LobbyBalance _lobbyBalance;

        public void Construct(IMapRepository mapRepository, IStaticDataService staticData,
            GameStateMachine stateMachine)
        {
            _mapRepository = mapRepository;
            _stateMachine = stateMachine;
            _lobbyBalance = staticData.GetLobbyBalance();
            _minGameTime = _lobbyBalance.minMatchDuration;
            _maxGameTime = _lobbyBalance.maxMatchDuration;
            InitGameDuration();
            InitMapChoice();
            resetButton.onClick.AddListener(OnResetButton);
            backButton.onClick.AddListener(OnBackButton);
            applyButton.onClick.AddListener(OnApplyButton);
            nextMapButton.onClick.AddListener(OnNextMapButton);
            previousMapButton.onClick.AddListener(OnPreviousButton);
        }

        private void InitMapChoice()
        {
            var configure = _mapRepository.GetCurrentMap();
            if (configure != null)
            {
                mapName.SetText(configure.Item1);
                mapImage.texture = configure.Item2.image;
            }
        }

        private void OnEnable()
        {
            ShowCursor();
        }

        private void OnDisable()
        {
            HideCursor();
        }

        private void OnDestroy()
        {
            resetButton.onClick.RemoveListener(OnResetButton);
            backButton.onClick.RemoveListener(OnBackButton);
            applyButton.onClick.RemoveListener(OnApplyButton);
            nextMapButton.onClick.RemoveListener(OnNextMapButton);
            previousMapButton.onClick.RemoveListener(OnPreviousButton);
        }

        private void InitGameDuration()
        {
            _timeLimitation = new Limitation(_minGameTime, _maxGameTime);
            _timeLimitation.CurrentValue.ValueChanged += value => gameDuration.SetText(value.ToString());
            incrementGameDuration.onClick.AddListener(_timeLimitation.Increment);
            decrementGameDuration.onClick.AddListener(_timeLimitation.Decrement);
            gameDuration.SetText(_timeLimitation.CurrentValue.Value.ToString());
        }

        private void OnResetButton()
        {
            _timeLimitation.Reset();
        }

        private void OnBackButton()
        {
            _stateMachine.Enter<MainMenuState>();
        }

        private void OnApplyButton()
        {
            var serverSettings = new ServerSettings(mapName.text, _timeLimitation.CurrentValue.Value,
                _lobbyBalance.spawnTime, _lobbyBalance.boxSpawnTime);
            if (Constants.isLocalBuild)
            {
                _stateMachine.Enter<StartMatchState, ServerSettings>(
                    serverSettings);
            }
            else
            {
                _stateMachine.Enter<StartSteamLobbyState, ServerSettings>(
                    serverSettings);
            }
        }

        private void OnNextMapButton()
        {
            var configure = _mapRepository.GetNextMap();
            if (configure != null)
            {
                mapName.SetText(configure.Item1);
                mapImage.texture = configure.Item2.image;
            }
        }

        private void OnPreviousButton()
        {
            var configure = _mapRepository.GetPreviousMap();
            if (configure != null)
            {
                mapName.SetText(configure.Item1);
                mapImage.texture = configure.Item2.image;
            }
        }
    }
}