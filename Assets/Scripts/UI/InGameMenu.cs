using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
	public class InGameMenu : MonoBehaviour
	{
		public event Action ResumeButtonPressed
		{
			add => resumeButton.onClick.AddListener(new UnityAction(value));
			remove => resumeButton.onClick.RemoveListener(new UnityAction(value));
		}

		public event Action SettingsButtonPressed
		{
			add => settingsButton.onClick.AddListener(new UnityAction(value));
			remove => settingsButton.onClick.RemoveListener(new UnityAction(value));
		}

		public event Action ExitButtonPressed
		{
			add => exitButton.onClick.AddListener(new UnityAction(value));
			remove => exitButton.onClick.RemoveListener(new UnityAction(value));
		}

		[SerializeField]
		private CanvasGroup canvasGroup;

		public CanvasGroup CanvasGroup => canvasGroup;

		[SerializeField]
		private Button resumeButton;

		[SerializeField]
		private Button settingsButton;

		[SerializeField]
		private Button exitButton;

		public void Construct()
		{
			canvasGroup.alpha = 0.0f;
		}
	}
}