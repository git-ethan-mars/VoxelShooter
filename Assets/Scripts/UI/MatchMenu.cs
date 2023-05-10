using Data;
using Infrastructure.States;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MatchMenu : MonoBehaviour
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private TextMeshProUGUI mapName;
        [Header("Player Number")] [SerializeField]
        private TextMeshProUGUI playersNumber;

        [SerializeField] private Button incrementPlayerNumber;
        [SerializeField] private Button decrementPlayerNumber;

        [Header("Game Duration")] [SerializeField]
        private TextMeshProUGUI gameDuration;

        [SerializeField] private Button incrementGameDuration;
        [SerializeField] private Button decrementGameDuration;
        private Limitation _playersLimitation;
        private Limitation _timeLimitation;
        private GameStateMachine _stateMachine;
        public const int MinGameTime = 5;
        public const int MaxGameTime = 15;
        public const int MinPlayersNumber = 2;
        public const int MaxPlayersNumber = 8;
        public const int SpawnTime = 7;
        private const string MainMenu = "MainMenu";
        private const string Main = "Main";


        public void Construct(GameStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            InitPlayersNumber();
            InitGameDuration();
            resetButton.onClick.AddListener(ResetLimitations);
            backButton.onClick.AddListener(() => stateMachine.Enter<MainMenuState, string>(MainMenu));
            applyButton.onClick.AddListener(() => _stateMachine.Enter<StartMatchState, string, ServerSettings>(Main,
                new ServerSettings(_timeLimitation.CurrentValue, _playersLimitation.CurrentValue, SpawnTime, mapName.text)));
            
        }

        private void InitGameDuration()
        {
            _timeLimitation = new Limitation(MinGameTime, MaxGameTime);
            _timeLimitation.OnCurrentValueUpdate += () => gameDuration.SetText(_timeLimitation.CurrentValue.ToString());
            incrementGameDuration.onClick.AddListener(_timeLimitation.Increment);
            decrementGameDuration.onClick.AddListener(_timeLimitation.Decrement);
            gameDuration.SetText(_timeLimitation.CurrentValue.ToString());
        }

        private void InitPlayersNumber()
        {
            _playersLimitation = new Limitation(MinPlayersNumber, MaxPlayersNumber);
            _playersLimitation.OnCurrentValueUpdate +=
                () => playersNumber.SetText(_playersLimitation.CurrentValue.ToString());
            incrementPlayerNumber.onClick.AddListener(_playersLimitation.Increment);
            decrementPlayerNumber.onClick.AddListener(_playersLimitation.Decrement);
            playersNumber.SetText(_playersLimitation.CurrentValue.ToString());
        }

        private void ResetLimitations()
        {
            _playersLimitation.Reset();
            _timeLimitation.Reset();
        }
    }
}