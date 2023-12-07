using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Windows
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField]
        private RawImage avatar;

        public RawImage Avatar => avatar;

        [SerializeField]
        private TextMeshProUGUI nickName;

        public TextMeshProUGUI NickName => nickName;

        [SerializeField]
        private TextMeshProUGUI kills;

        public TextMeshProUGUI Kills => kills;

        [SerializeField]
        private TextMeshProUGUI deaths;

        public TextMeshProUGUI Deaths => deaths;

        [SerializeField]
        private TextMeshProUGUI classText;

        public TextMeshProUGUI ClassText => classText;
    }
}