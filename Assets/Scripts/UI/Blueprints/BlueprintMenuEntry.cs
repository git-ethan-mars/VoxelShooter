using Data;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class BlueprintMenuEntry : MonoBehaviour
	{
		private const float HighlightedScale = 1.15f;
		private const float ScaleDuration = 0.08f;
		private static readonly Color HighlightedColor = new Color(0.72f, 0.95f, 0.62f, 1.0f);

		[SerializeField] private Image background;
		[SerializeField] private TextMeshProUGUI layoutName;
		[SerializeField] private TextMeshProUGUI blockCount;

		private bool _isHighlighted;

		private void OnDisable()
		{
			transform.DOKill();
		}

		public void Initialize(BlueprintLayout layout)
		{
			layoutName.SetText(layout.Name);
			blockCount.SetText(layout.Positions.Count.ToString());
			_isHighlighted = true;
			SetHighlighted(false);
		}

		public void SetHighlighted(bool isHighlighted)
		{
			if (_isHighlighted == isHighlighted)
			{
				return;
			}

			_isHighlighted = isHighlighted;
			background.color = isHighlighted ? HighlightedColor : Color.white;
			transform.DOKill();
			transform.DOScale(isHighlighted ? HighlightedScale : 1.0f, ScaleDuration).SetUpdate(true);
		}
	}
}
