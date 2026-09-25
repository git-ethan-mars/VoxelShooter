using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class PaletteElementView : MonoBehaviour
	{
		private const float SelectedScale = 1.2f;
		private const float ScaleDuration = 0.1f;

		[SerializeField] private Image colorIcon;
		[SerializeField] private GameObject selection;

		public Color32 Color { get; private set; }

		private void OnDisable()
		{
			transform.DOKill();
		}

		public void Construct(Color32 color)
		{
			Color = color;
			colorIcon.color = color;
			selection.SetActive(false);
			transform.DOKill();
			transform.localScale = Vector3.one;
		}

		public void SetSelected(bool isSelected)
		{
			if (selection.activeSelf == isSelected)
			{
				return;
			}

			selection.SetActive(isSelected);
			transform.DOKill();
			transform.DOScale(isSelected ? SelectedScale : 1.0f, ScaleDuration);
		}
	}
}
