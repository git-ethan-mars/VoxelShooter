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
        [SerializeField] private List<ScoreUI> scores;
        private IInputService _inputService;
        private CanvasGroup _canvasGroup;
        private IAvatarLoader _avatarLoader;


        public void Construct(IInputService inputService, IAvatarLoader avatarLoader,
            CustomNetworkManager networkManager)
        {
            networkManager.ScoreboardChanged += UpdateScoreboard;
            networkManager.GameFinished += ShowFinalStatistics;
            _inputService = inputService;
            _avatarLoader = avatarLoader;
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
        }

        private void Update()
        {
            _canvasGroup.alpha = _inputService.IsScoreboardButtonHold() ? 1 : 0;
        }

        private void ShowFinalStatistics()
        {
            enabled = false;
            _canvasGroup.alpha = 1;
        }

        private void UpdateScoreboard(List<ScoreData> scoreBoardData)
        {
            for (var i = 0; i < scores.Count; i++)
            {
                scores[i].gameObject.SetActive(false);
            }

            for (var i = 0; i < scoreBoardData.Count; i++)
            {
                scores[i].nickName.SetText(scoreBoardData[i].NickName);
                scores[i].kills.SetText(scoreBoardData[i].Kills.ToString());
                scores[i].deaths.SetText(scoreBoardData[i].Deaths.ToString());
                scores[i].classText.SetText(scoreBoardData[i].GameClass.ToString());
                scores[i].avatar.texture = _avatarLoader.RequestAvatar(scoreBoardData[i].SteamID);;
                scores[i].gameObject.SetActive(true);
            }
        }
    }
}