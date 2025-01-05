using System;
using GamePlay.Data;
using GamePlay.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VoxelMap;

namespace UI
{
    public class MatchMenu : MonoBehaviour
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

        public event Action BackButtonPressed
        {
            add => backButton.onClick.AddListener(new UnityAction(value));
            remove => backButton.onClick.RemoveListener(new UnityAction(value));
        }

        public event Action<WorldSettings> ApplyButtonPressed;

        private Limitation _timeLimitation;

        private IMapRepository _mapRepository;

        private int _minGameTime;

        private int _maxGameTime;

        private LobbyBalance _lobbyBalance;

        public void Construct(IMapRepository mapRepository, IStaticDataService staticData)
        {
            _mapRepository = mapRepository;
            _lobbyBalance = staticData.GetLobbyBalance();
            _minGameTime = _lobbyBalance.minMatchDuration;
            _maxGameTime = _lobbyBalance.maxMatchDuration;
            InitGameDuration();
            InitMapChoice();
            applyButton.onClick.AddListener(OnApplyButtonPressed);
            resetButton.onClick.AddListener(OnResetButton);
            nextMapButton.onClick.AddListener(OnNextMapButton);
            previousMapButton.onClick.AddListener(OnPreviousButton);
        }

        private void InitMapChoice()
        {
            var configure = _mapRepository.GetCurrentMap();
            if (configure != null)
            {
                mapName.SetText(configure.Item1);
                mapImage.texture = configure.Item2.Image;
            }
        }

        private void OnDestroy()
        {
            applyButton.onClick.RemoveListener(OnApplyButtonPressed);
            resetButton.onClick.RemoveListener(OnResetButton);
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

        private void OnNextMapButton()
        {
            var configure = _mapRepository.GetNextMap();
            if (configure != null)
            {
                mapName.SetText(configure.Item1);
                mapImage.texture = configure.Item2.Image;
            }
        }

        private void OnPreviousButton()
        {
            var configure = _mapRepository.GetPreviousMap();
            if (configure != null)
            {
                mapName.SetText(configure.Item1);
                mapImage.texture = configure.Item2.Image;
            }
        }

        private void OnApplyButtonPressed()
        {
            var worldSettings = new WorldSettings(mapName.text, _timeLimitation.CurrentValue.Value,
                _lobbyBalance.spawnTime, _lobbyBalance.spawnTime);
            ApplyButtonPressed?.Invoke(worldSettings);
        }
    }
}