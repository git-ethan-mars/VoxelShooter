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
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private List<ScoreUI> scores;

        private IInputService _inputService;
        private IAvatarLoader _avatarLoader;
        private IClient _client;


        public void Construct(IClient client, IInputService inputService,
            IAvatarLoader avatarLoader)
        {
            _client = client;
            _client.ScoreboardChanged += UpdateScoreboard;
            _client.GameFinished += ShowFinalStatistics;
            _inputService = inputService;
            _avatarLoader = avatarLoader;
            canvasGroup.alpha = 0;
        }

        private void Update()
        {
            canvasGroup.alpha = _inputService.IsScoreboardButtonHold() ? 1 : 0;
        }

        private void ShowFinalStatistics()
        {
            _client.GameFinished -= ShowFinalStatistics;
            enabled = false;
            canvasGroup.alpha = 1;
        }

        private void UpdateScoreboard(List<ScoreData> scoreBoardData)
        {
            for (var i = 0; i < scores.Count; i++)
            {
                scores[i].gameObject.SetActive(false);
            }

            for (var i = 0; i < scoreBoardData.Count; i++)
            {
                scores[i].NickName.SetText(scoreBoardData[i].NickName);
                scores[i].Kills.SetText(scoreBoardData[i].Kills.ToString());
                scores[i].Deaths.SetText(scoreBoardData[i].Deaths.ToString());
                scores[i].ClassText.SetText(scoreBoardData[i].GameClass.ToString());
                scores[i].Avatar.texture = _avatarLoader.RequestAvatar(scoreBoardData[i].SteamID);
                scores[i].gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            _client.ScoreboardChanged -= UpdateScoreboard;
        }
    }
}