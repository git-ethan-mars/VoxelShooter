using System.Collections.Generic;
using Data;
using Infrastructure.Services.PlayerDataLoader;
using Networking;
using UnityEngine;

namespace UI.InGameUI
{
    public class Scoreboard : MonoBehaviour
    {
        public CanvasGroup CanvasGroup => canvasGroup;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private List<ScoreUI> scores;

        private IAvatarLoader _avatarLoader;
        private CustomNetworkManager _networkManager;

        public void Construct(CustomNetworkManager networkManager, IAvatarLoader avatarLoader)
        {
            _networkManager = networkManager;
            _networkManager.Client.ScoreboardChanged += UpdateScoreboard;
            _avatarLoader = avatarLoader;
            canvasGroup.alpha = 0;
        }

        private void ShowFinalStatistics()
        {
            enabled = false;
            canvasGroup.alpha = 1;
        }

        private void UpdateScoreboard(List<ScoreData> scoreboardData)
        {
            for (var i = 0; i < scores.Count; i++)
            {
                scores[i].gameObject.SetActive(false);
            }

            for (var i = 0; i < scoreboardData.Count; i++)
            {
                scores[i].NickName.SetText(scoreboardData[i].NickName);
                scores[i].Kills.SetText(scoreboardData[i].Kills.ToString());
                scores[i].Deaths.SetText(scoreboardData[i].Deaths.ToString());
                scores[i].ClassText.SetText(scoreboardData[i].GameClass.ToString());
                scores[i].Avatar.texture = _avatarLoader.RequestAvatar(scoreboardData[i].SteamID);
                scores[i].gameObject.SetActive(true);
            }
        }

        private void OnDestroy()
        {
            _networkManager.Client.ScoreboardChanged -= UpdateScoreboard;
        }
    }
}