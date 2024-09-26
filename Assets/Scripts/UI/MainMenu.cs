using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
	public class MainMenu : MonoBehaviour
	{
		public event Action JoinButtonPressed
		{
			add => joinMatchButton.onClick.AddListener(new UnityAction(value));
			remove => joinMatchButton.onClick.RemoveListener(new UnityAction(value));
		}
		
		public event Action CreateMatchButtonPressed
		{
			add => createMatchButton.onClick.AddListener(new UnityAction(value));
			remove => createMatchButton.onClick.RemoveListener(new UnityAction(value));
		}
		public event Action SettingButtonPressed
		{
			add => settingButton.onClick.AddListener(new UnityAction(value));
			remove => settingButton.onClick.RemoveListener(new UnityAction(value));
		}
		public event Action ExitButtonPressed
		{
			add => exitButton.onClick.AddListener(new UnityAction(value));
			remove => exitButton.onClick.RemoveListener(new UnityAction(value));
		}

		[SerializeField]
		private Button joinMatchButton;

		[SerializeField]
		private Button createMatchButton;

		[SerializeField]
		private Button settingButton;

		[SerializeField]
		private Button exitButton;
	}
}