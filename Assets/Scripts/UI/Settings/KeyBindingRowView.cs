using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class KeyBindingRowView : MonoBehaviour
	{
		private static readonly Color ListeningColor = new Color(0.72f, 0.95f, 0.62f, 1.0f);

		[SerializeField] private Image background;
		[SerializeField] private TextMeshProUGUI label;
		[SerializeField] private Button keyButton;
		[SerializeField] private Image keyBackground;
		[SerializeField] private TextMeshProUGUI keyText;

		private Color _keyTextColor;

		public Observable<Unit> Clicked => keyButton.OnClickAsObservable();

		private void Awake()
		{
			_keyTextColor = keyText.color;
		}

		public void InitializeAction(string actionName)
		{
			label.SetText(actionName);
			background.enabled = true;
			keyButton.gameObject.SetActive(true);
		}

		public void InitializeHeader(string title)
		{
			label.SetText(title);
			background.enabled = false;
			keyButton.gameObject.SetActive(false);
		}

		public void SetKey(string keyName)
		{
			keyText.SetText(keyName);
		}

		public void SetListening(bool isListening)
		{
			keyBackground.color = isListening ? ListeningColor : Color.white;
			keyText.color = isListening ? Color.black : _keyTextColor;
		}
	}
}
