using Data;
using Infrastructure.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MatchMenu : Window
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private TextMeshProUGUI mapName;

        [Header("Spawn Time")] [SerializeField]
        private TextMeshProUGUI spawnTimeText;

        [SerializeField] private Button incrementPlayerNumber;
        [SerializeField] private Button decrementPlayerNumber;

        [Header("Game Duration")] [SerializeField]
        private TextMeshProUGUI gameDuration;

        [SerializeField] private Button incrementGameDuration;
        [SerializeField] private Button decrementGameDuration;
        private Limitation _spawnTimeLimitation;
        private Limitation _timeLimitation;
        private GameStateMachine _stateMachine;
        private const int MinGameTime = 1;
        private const int MaxGameTime = 15;
        private const int MinSpawnTime = 1;
        private const int MaxSpawnTime = 10;


        public void Construct(GameStateMachine stateMachine, bool isLocalBuild)
        {
            _stateMachine = stateMachine;
            InitSpawnTime();
            InitGameDuration();
            resetButton.onClick.AddListener(ResetLimitations);
            backButton.onClick.AddListener(stateMachine.Enter<MainMenuState>);
            if (isLocalBuild)
                applyButton.onClick.AddListener(() =>
                    _stateMachine.Enter<StartMatchState, ServerSettings>(
                        new ServerSettings(_timeLimitation.CurrentValue, _spawnTimeLimitation.CurrentValue,
                            mapName.text)));
            else
            {
                applyButton.onClick.AddListener(() => _stateMachine.Enter<StartSteamLobbyState, ServerSettings>(
                    new ServerSettings(_timeLimitation.CurrentValue, _spawnTimeLimitation.CurrentValue, mapName.text)));
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

        private void ResetLimitations()
        {
            _spawnTimeLimitation.Reset();
            _timeLimitation.Reset();
        }
    }
}