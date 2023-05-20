using System.Collections.Generic;
using Data;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Networking;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Scoreboard : MonoBehaviour
    {
        [SerializeField] private List<Score> scores;
        private IInputService _inputService;
        private CanvasGroup _canvasGroup;
        private IAvatarLoader _avatarLoader;


        public void Construct(IInputService inputService, IAvatarLoader playerDataLoader,
            CustomNetworkManager networkManager)
        {
            networkManager.ScoreboardChanged += UpdateScoreboard;
            _inputService = inputService;
            _avatarLoader = playerDataLoader;
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }

        private void Update()
        {
            _canvasGroup.alpha = _inputService.IsScoreboardButtonHold() ? 1 : 0;
        }

        private void UpdateScoreboard(List<ScoreData> scoreData)
        {
            for (var i = 0; i < scores.Count; i++)
            {
                scores[i].gameObject.SetActive(false);
            }

            for (var i = 0; i < scoreData.Count; i++)
            {
                var steamAvatar = _avatarLoader.LoadAvatar(scoreData[i].SteamID);
                scores[i].avatar.texture = steamAvatar;
                scores[i].nickName.SetText(scoreData[i].NickName);
                scores[i].kills.SetText(scoreData[i].Kills.ToString());
                scores[i].deaths.SetText(scoreData[i].Deaths.ToString());
                scores[i].classText.SetText(scoreData[i].GameClass.ToString());
                scores[i].gameObject.SetActive(true);
            }
        }
    }
}