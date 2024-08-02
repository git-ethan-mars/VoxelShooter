using PlayerLogic;
using TMPro;
using UnityEngine;

namespace UI
{
    public class BlockCounter : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI blockCountText;

        private Player _player;

        public void Construct(Player player)
        {
            _player = player;
            blockCountText.SetText(_player.BlockCount.Value.ToString());
            _player.BlockCount.ValueChanged += OnBlockCountChange;
        }

        private void OnBlockCountChange(int currentBlockCount)
        {
            blockCountText.SetText(currentBlockCount.ToString());
        }

        private void OnDestroy()
        {
            _player.BlockCount.ValueChanged -= OnBlockCountChange;
        }
    }
}