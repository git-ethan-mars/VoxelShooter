using System;
using GamePlay.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(CanvasGroup))]
	public class ChooseClassMenu : MonoBehaviour
	{
		public event Action<GameClass> ChangeClassButtonPressed;

		[SerializeField]
		private CanvasGroup canvasGroup;

		public CanvasGroup CanvasGroup => canvasGroup;

		[SerializeField]
		private Button builderButton;

		[SerializeField]
		private Button sniperButton;

		[SerializeField]
		private Button combatantButton;

		[SerializeField]
		private Button grenadierButton;

		[SerializeField]
		private Button exitButton;

		public void Construct()
		{
			builderButton.onClick.AddListener(ChooseBuilder);
			sniperButton.onClick.AddListener(ChooseSniper);
			combatantButton.onClick.AddListener(ChooseCombatant);
			grenadierButton.onClick.AddListener(ChooseGrenadier);
			canvasGroup.alpha = 0.0f;
		}

		private void ChooseBuilder()
		{
			ChangeClassButtonPressed?.Invoke(GameClass.Builder);
		}

		private void ChooseSniper()
		{
			ChangeClassButtonPressed?.Invoke(GameClass.Sniper);
		}

		private void ChooseCombatant()
		{
			ChangeClassButtonPressed?.Invoke(GameClass.Combatant);
		}

		private void ChooseGrenadier()
		{
			ChangeClassButtonPressed?.Invoke(GameClass.Grenadier);
		}

		private void OnDestroy()
		{
			builderButton.onClick.RemoveListener(ChooseBuilder);
			sniperButton.onClick.RemoveListener(ChooseSniper);
			combatantButton.onClick.RemoveListener(ChooseCombatant);
			grenadierButton.onClick.RemoveListener(ChooseGrenadier);
		}
	}
}