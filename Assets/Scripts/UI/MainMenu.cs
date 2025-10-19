using R3;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class MainMenu : MonoBehaviour, IBaseMenu
	{
		[field: SerializeField] public Button CreateMatchButton { get; private set; }
		[field: SerializeField] public Button JoinMatchButton { get; private set; }
		[field: SerializeField] public Button SettingsButton { get; private set; }
		[field: SerializeField] public CanvasGroup CanvasGroup { get; private set; }
		[SerializeField] private Button exitButton;

		private void OnEnable()
		{
			CreateMatchButtonPressed = CreateMatchButton.onClick.AsObservable();
			JoinButtonPressed = JoinMatchButton.onClick.AsObservable();
			SettingsButtonPressed = SettingsButton.onClick.AsObservable();
			ExitButtonPressed = exitButton.onClick.AsObservable();
		}

		public Observable<Unit> CreateMatchButtonPressed { get; private set; }
		public Observable<Unit> JoinButtonPressed { get; private set; }
		public Observable<Unit> SettingsButtonPressed { get; private set; }
		public Observable<Unit> ExitButtonPressed { get; private set; }

		public void Show()
		{
		}

		public void Hide()
		{
		}
	}
}