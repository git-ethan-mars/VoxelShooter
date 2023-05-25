using System.Collections.Generic;
using Data;
using Infrastructure.Services.Input;
using Infrastructure.Services.PlayerDataLoader;
using Networking;
using Steamworks;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Scoreboard : MonoBehaviour
    {
        [SerializeField] private List<ScoreUI> scores;
        private Dictionary<CSteamID, Texture2D> _avatarBySteamId;
        private IInputService _inputService;
        private CanvasGroup _canvasGroup;
        private IAvatarLoader _avatarLoader;
        private List<ScoreData> _currentScoreboardData;


        public void Construct(IInputService inputService, IAvatarLoader playerDataLoader,
            CustomNetworkManager networkManager)
        {
            networkManager.ScoreboardChanged += UpdateScoreboard;
            _inputService = inputService;
            _avatarLoader = playerDataLoader;
            _avatarLoader.OnAvatarLoaded += ApplyAvatar;
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.alpha = 0;
            _avatarBySteamId = new Dictionary<CSteamID, Texture2D>();
        }

        private void Update()
        {
            _canvasGroup.alpha = _inputService.IsScoreboardButtonHold() ? 1 : 0;
        }
        private void UpdateScoreboard(List<ScoreData> scoreBoardData)
        {
            _currentScoreboardData = scoreBoardData;
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
                if (_avatarBySteamId.TryGetValue(scoreBoardData[i].SteamID, out var avatarTexture))
                    scores[i].avatar.texture = avatarTexture;
                else
                {
                    _avatarLoader.RequestAvatar(scoreBoardData[i].SteamID);
                }
                scores[i].gameObject.SetActive(true);
            }
        }

        private void ApplyAvatar(CSteamID steamID, Texture2D steamAvatar)
        {
            _avatarBySteamId[steamID] = steamAvatar;
        }
    }
}