using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Data;
using R3;
using Reflex.Attributes;
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
			mainMenu.JoinButtonPressed.Subscribe(_ => OnJoinMatchButtonPressed()).AddTo(mainMenu);
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
#if !LOCAL_BUILD
			SwapWindowsLeftward(joinMatchMenu, mainMenu).Forget();
#endif
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

		private async UniTaskVoid SwapWindowsLeftward(IBaseMenu nextWindow, IBaseMenu previousWindow)
		{
			nextWindow.transform.localPosition = new Vector3(canvas.renderingDisplaySize.x, 0, 0);
			nextWindow.CanvasGroup.interactable = true;
			nextWindow.CanvasGroup.blocksRaycasts = true;
			UniTask nextWindowAnimation = DOTween.Sequence()
				.Append(nextWindow.transform.DOLocalMoveX(0, AnimationDuration))
				.Insert(0, nextWindow.CanvasGroup.DOFade(1, AnimationDuration))
				.ToUniTask();
			nextWindow.Show();

			previousWindow.CanvasGroup.interactable = false;
			previousWindow.CanvasGroup.blocksRaycasts = false;
			UniTask previousWindowAnimation = DOTween.Sequence()
				.Append(previousWindow.transform.DOLocalMoveX(-canvas.renderingDisplaySize.x, AnimationDuration))
				.Insert(0, previousWindow.CanvasGroup.DOFade(0, AnimationDuration))
				.ToUniTask();
			previousWindow.Hide();
			await UniTask.WhenAll(nextWindowAnimation, previousWindowAnimation);
		}

		private async UniTaskVoid SwapWindowsRightward(IBaseMenu nextWindow, IBaseMenu previousWindow)
		{
			nextWindow.transform.localPosition = new Vector3(-canvas.renderingDisplaySize.x, 0, 0);
			nextWindow.CanvasGroup.interactable = true;
			nextWindow.CanvasGroup.blocksRaycasts = true;
			UniTask nextWindowAnimation = DOTween.Sequence()
				.Append(nextWindow.transform.DOLocalMoveX(0, AnimationDuration))
				.Insert(0, nextWindow.CanvasGroup.DOFade(1, AnimationDuration))
				.ToUniTask();
			nextWindow.Show();

			previousWindow.CanvasGroup.interactable = false;
			previousWindow.CanvasGroup.blocksRaycasts = false;
			UniTask previousWindowAnimation = DOTween.Sequence()
				.Append(previousWindow.transform.DOLocalMoveX(canvas.renderingDisplaySize.x, AnimationDuration))
				.Insert(0, previousWindow.CanvasGroup.DOFade(0, AnimationDuration))
				.ToUniTask();
			previousWindow.Hide();
			await UniTask.WhenAll(nextWindowAnimation, previousWindowAnimation);
		}
	}
}