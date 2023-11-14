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

        [Header("Spawn Time")]
        [SerializeField]
        private TextMeshProUGUI spawnTimeText;

        [SerializeField]
        private Button incrementPlayerNumber;

        [SerializeField]
        private Button decrementPlayerNumber;

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

        [Header("Box spawn time")]
        [SerializeField]
        private TextMeshProUGUI boxSpawnTime;

        [SerializeField]
        private Button incrementBoxSpawnTime;

        [SerializeField]
        private Button decrementBoxSpawnTime;

        private Limitation _spawnTimeLimitation;
        private Limitation _timeLimitation;
        private Limitation _boxSpawnTimeLimitation;
        private IMapRepository _mapRepository;
        private GameStateMachine _stateMachine;
        private const int MinGameTime = 5;
        private const int MaxGameTime = 10;
        private const int MinSpawnTime = 3;
        private const int MaxSpawnTime = 7;
        private const int MinBoxSpawnTime = 10;
        private const int MaxBoxSpawnTime = 20;

        public void Construct(IMapRepository mapRepository, GameStateMachine stateMachine)
        {
            _mapRepository = mapRepository;
            _stateMachine = stateMachine;
            InitSpawnTime();
            InitGameDuration();
            InitMapChoice();
            InitBoxSpawnTime();
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
            _timeLimitation = new Limitation(MinGameTime, MaxGameTime);
            _timeLimitation.OnCurrentValueUpdate += () => gameDuration.SetText(_timeLimitation.CurrentValue.ToString());
            incrementGameDuration.onClick.AddListener(_timeLimitation.Increment);
            decrementGameDuration.onClick.AddListener(_timeLimitation.Decrement);
            gameDuration.SetText(_timeLimitation.CurrentValue.ToString());
        }

        private void InitSpawnTime()
        {
            _spawnTimeLimitation = new Limitation(MinSpawnTime, MaxSpawnTime);
            _spawnTimeLimitation.OnCurrentValueUpdate +=
                () => spawnTimeText.SetText(_spawnTimeLimitation.CurrentValue.ToString());
            incrementPlayerNumber.onClick.AddListener(_spawnTimeLimitation.Increment);
            decrementPlayerNumber.onClick.AddListener(_spawnTimeLimitation.Decrement);
            spawnTimeText.SetText(_spawnTimeLimitation.CurrentValue.ToString());
        }

        private void InitBoxSpawnTime()
        {
            _boxSpawnTimeLimitation = new Limitation(MinBoxSpawnTime, MaxBoxSpawnTime);
            _boxSpawnTimeLimitation.OnCurrentValueUpdate +=
                () => boxSpawnTime.SetText(_boxSpawnTimeLimitation.CurrentValue.ToString());
            incrementBoxSpawnTime.onClick.AddListener(_boxSpawnTimeLimitation.Increment);
            decrementBoxSpawnTime.onClick.AddListener(_boxSpawnTimeLimitation.Decrement);
            boxSpawnTime.SetText(_boxSpawnTimeLimitation.CurrentValue.ToString());
        }

        private void OnResetButton()
        {
            _spawnTimeLimitation.Reset();
            _timeLimitation.Reset();
        }

        private void OnBackButton()
        {
            _stateMachine.Enter<MainMenuState>();
        }

        private void OnApplyButton()
        {
            var serverSettings = new ServerSettings(mapName.text, _timeLimitation.CurrentValue,
                _spawnTimeLimitation.CurrentValue, _boxSpawnTimeLimitation.CurrentValue);
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