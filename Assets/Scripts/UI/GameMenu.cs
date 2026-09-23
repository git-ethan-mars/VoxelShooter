using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Data;
using DG.Tweening;
using R3;
using Reflex.Attributes;
using Services;
using Services.ServerList;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
namespace UI
{
	public class GameMenu : MonoBehaviour
	{
		private const float AnimationDuration = 0.5f;

		[SerializeField] private Canvas canvas;
		[SerializeField] private List<Sprite> backgroundSprites;
		[SerializeField] private Image backgroundImage;

		[SerializeField] private MainMenu mainMenu;
		[SerializeField] private JoinMatchMenu joinMatchMenu;
		[SerializeField] private SettingsMenu settingsMenu;
		[SerializeField] private MatchMenu matchMenu;

		private EventSystem _eventSystem;

		public Observable<GameSettings> CreateGameRequested => matchMenu.ApplyButtonPressed.AsObservable();
#if LOCAL_BUILD
		public Observable<Unit> JoinButtonPressed => mainMenu.JoinButtonPressed.AsObservable();
#else
		public Observable<Server> JoinServerButtonPressed => joinMatchMenu.JoinServerButtonPressed;
#endif

		[Inject]
		private void Construct()
		{
			_eventSystem = EventSystem.current;

			if (_eventSystem == null)
			{
				throw new NullReferenceException($"Couldn't find {nameof(EventSystem)} on scene");
			}

			Sprite backgroundSprite = backgroundSprites[Random.Range(0, backgroundSprites.Count)];
			backgroundImage.sprite = backgroundSprite;

			EventSystem.current.firstSelectedGameObject = mainMenu.CreateMatchButton.gameObject;

			mainMenu.CreateMatchButtonPressed.Subscribe(_ => OnCreateMatchButtonPressed()).AddTo(mainMenu);
#if !LOCAL_BUILD
			mainMenu.JoinButtonPressed.Subscribe(_ => OnJoinMatchButtonPressed()).AddTo(mainMenu);
#endif
			joinMatchMenu.BackButtonPressed.Subscribe(_ => OnJoinMatchMenuBackButtonPressed()).AddTo(joinMatchMenu);
			mainMenu.SettingsButtonPressed.Subscribe(_ => OnSettingsButtonPressed()).AddTo(mainMenu);
			mainMenu.ExitButtonPressed.Subscribe(_ => Application.Quit()).AddTo(mainMenu);
			settingsMenu.BackButtonPressed.Subscribe(_ => OnSettingsMenuBackButtonPressed()).AddTo(settingsMenu);
			matchMenu.BackButtonPressed.Subscribe(_ => OnMatchMenuBackButtonPressed()).AddTo(matchMenu);
		}

		private void OnCreateMatchButtonPressed()
		{
			SwapWindowsLeftward(matchMenu, mainMenu).Forget();
		}

		private void OnJoinMatchButtonPressed()
		{
			SwapWindowsLeftward(joinMatchMenu, mainMenu).Forget();
		}

		private void OnJoinMatchMenuBackButtonPressed()
		{
			SwapWindowsRightward(mainMenu, joinMatchMenu).Forget();
		}

		private void OnSettingsButtonPressed()
		{
			SwapWindowsLeftward(settingsMenu, mainMenu).Forget();
		}

		private void OnSettingsMenuBackButtonPressed()
		{
			SwapWindowsRightward(mainMenu, settingsMenu).Forget();
			_eventSystem.SetSelectedGameObject(mainMenu.SettingsButton.gameObject);
		}

		private void OnMatchMenuBackButtonPressed()
		{
			SwapWindowsRightward(mainMenu, matchMenu).Forget();
			_eventSystem.SetSelectedGameObject(mainMenu.CreateMatchButton.gameObject);
		}

		private async UniTaskVoid SwapWindowsLeftward(BaseMenu nextWindow, BaseMenu previousWindow)
		{
			nextWindow.transform.localPosition = new Vector3(canvas.renderingDisplaySize.x + ((RectTransform)nextWindow.transform).rect.width / 2,
				0, 0);
			nextWindow.CanvasGroup.blocksRaycasts = true;
			nextWindow.CanvasGroup.interactable = true;
			nextWindow.CanvasGroup.alpha = 1;
			previousWindow.CanvasGroup.blocksRaycasts = false;
			nextWindow.Show();

			var nextWindowAnimation = nextWindow.transform.DOLocalMoveX(0, AnimationDuration).ToUniTask();
			var previousWindowAnimation = previousWindow.transform.DOLocalMoveX(
					-canvas.renderingDisplaySize.x - ((RectTransform)previousWindow.transform).rect.width,
					AnimationDuration)
				.ToUniTask();
			await UniTask.WhenAll(nextWindowAnimation, previousWindowAnimation);

			previousWindow.Hide();
		}

		private async UniTaskVoid SwapWindowsRightward(BaseMenu nextWindow, BaseMenu previousWindow)
		{
			nextWindow.transform.localPosition = new Vector3(-canvas.renderingDisplaySize.x - ((RectTransform)nextWindow.transform).rect.width / 2, 0, 0);
			nextWindow.CanvasGroup.blocksRaycasts = true;
			nextWindow.CanvasGroup.interactable = true;
			nextWindow.CanvasGroup.alpha = 1;
			previousWindow.CanvasGroup.blocksRaycasts = false;
			nextWindow.Show();

			var nextWindowAnimation = nextWindow.transform.DOLocalMoveX(0, AnimationDuration).ToUniTask();
			var previousWindowAnimation = previousWindow.transform.DOLocalMoveX(canvas.renderingDisplaySize.x + ((RectTransform)previousWindow
				.transform).rect.width / 2, 
					AnimationDuration).ToUniTask();
			await UniTask.WhenAll(nextWindowAnimation, previousWindowAnimation);

			previousWindow.Hide();
		}
	}
}