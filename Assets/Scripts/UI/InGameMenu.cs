using R3;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	public class InGameMenu : MonoBehaviour
	{
		[SerializeField] private Button resumeButton;
		[SerializeField] private Button settingsButton;
		[SerializeField] private Button exitButton;

		private void Awake()
		{
			ResumeButtonPressed = resumeButton.onClick.AsObservable();
			SettingsButtonPressed = settingsButton.onClick.AsObservable();
			ExitButtonPressed = exitButton.onClick.AsObservable();
		}

		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
		public Observable<Unit> ResumeButtonPressed { get; private set; }
		public Observable<Unit> SettingsButtonPressed { get; private set; }
		public Observable<Unit> ExitButtonPressed { get; private set; }
	}
}