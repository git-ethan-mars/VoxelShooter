using System.Collections.Generic;
using System.Text.RegularExpressions;
using GamePlay;
using Networking.Messages;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	public class ChatView : MonoBehaviour
	{
		private const int MaxLines = 100;
		private const float ScrollStep = 60.0f;
		private const float LineLifetime = 8.0f;
		private const float FadeDuration = 1.0f;
		private const string PlayerColor = "#F2D16B";
		private const string SystemColor = "#A8D08D";
		private const string ReplyColor = "#C9C3A6";

		private static readonly Regex NoParseEnd = new Regex("</noparse>", RegexOptions.IgnoreCase);

		[SerializeField] private ScrollRect scrollRect;
		[SerializeField] private RectTransform messagesContainer;
		[SerializeField] private CanvasGroup linePrefab;
		[SerializeField] private CanvasGroup inputGroup;
		[SerializeField] private TMP_InputField inputField;
		[SerializeField] private CanvasGroup scrollbarGroup;
		[SerializeField, Min(0)] private int sentHistorySize = 15;

		private readonly Queue<(CanvasGroup Line, float Time)> _lines = new Queue<(CanvasGroup Line, float Time)>();
		private readonly Subject<string> _submitted = new Subject<string>();
		private readonly List<string> _sentHistory = new List<string>();

		private bool _isOpen;
		private int _historyIndex;
		private string _draft;

		public Observable<string> Submitted => _submitted;

		public void Initialize(Chat chat)
		{
			chat.MessageReceived.Subscribe(AddMessage).AddTo(this);
			inputField.characterLimit = Chat.MaxMessageLength;
			inputField.onSubmit.AddListener(OnSubmit);
			Close();
		}

		private void Update()
		{
			foreach ((CanvasGroup line, float time) in _lines)
			{
				float age = Time.unscaledTime - time;
				line.alpha = _isOpen ? 1.0f : Mathf.Clamp01((LineLifetime + FadeDuration - age) / FadeDuration);
			}

			if (_isOpen)
			{
				Scroll();
			}
			else
			{
				scrollRect.verticalNormalizedPosition = 0.0f;
			}

			// Typing goes on until the chat is closed, even if the field lost focus, and never goes on after that.
			if (_isOpen && !inputField.isFocused)
			{
				inputField.ActivateInputField();
			}
			else if (!_isOpen && inputField.isFocused)
			{
				inputField.DeactivateInputField();
			}
		}

		// Runs after the input field handled the arrows itself, so the caret ends up after the recalled text.
		private void LateUpdate()
		{
			if (!_isOpen)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				Recall(-1);
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				Recall(1);
			}
		}

		public void Open()
		{
			_isOpen = true;
			_historyIndex = _sentHistory.Count;
			inputGroup.alpha = 1.0f;
			inputGroup.interactable = true;
			inputGroup.blocksRaycasts = true;
			scrollbarGroup.alpha = 1.0f;
			inputField.text = string.Empty;
			inputField.ActivateInputField();
		}

		public void Close()
		{
			_isOpen = false;
			inputGroup.alpha = 0.0f;
			inputGroup.interactable = false;
			inputGroup.blocksRaycasts = false;
			scrollbarGroup.alpha = 0.0f;
			inputField.text = string.Empty;
			inputField.DeactivateInputField();

			if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == inputField.gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}

		private void OnSubmit(string text)
		{
			text = text.Trim();

			if (text.Length > 0 && sentHistorySize > 0 && (_sentHistory.Count == 0 || _sentHistory[^1] != text))
			{
				_sentHistory.Add(text);

				if (_sentHistory.Count > sentHistorySize)
				{
					_sentHistory.RemoveRange(0, _sentHistory.Count - sentHistorySize);
				}
			}

			_submitted.OnNext(text);
		}

		// Steps through the messages sent before; stepping past the newest one brings back the unsent text.
		private void Recall(int step)
		{
			int index = Mathf.Clamp(_historyIndex + step, 0, _sentHistory.Count);

			if (index == _historyIndex)
			{
				return;
			}

			if (_historyIndex == _sentHistory.Count)
			{
				_draft = inputField.text;
			}

			_historyIndex = index;
			inputField.text = index < _sentHistory.Count ? _sentHistory[index] : _draft;
			inputField.MoveTextEnd(false);
		}

		// The cursor stays locked while typing, so the history is scrolled by the wheel and Page Up/Down wherever the pointer is.
		private void Scroll()
		{
			float viewHeight = scrollRect.viewport.rect.height;
			float hiddenHeight = messagesContainer.rect.height - viewHeight;

			if (hiddenHeight <= 0.0f)
			{
				return;
			}

			float offset = Input.mouseScrollDelta.y * ScrollStep;

			if (Input.GetKeyDown(KeyCode.PageUp))
			{
				offset += viewHeight;
			}

			if (Input.GetKeyDown(KeyCode.PageDown))
			{
				offset -= viewHeight;
			}

			if (offset != 0.0f)
			{
				scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + offset / hiddenHeight);
			}
		}

		private void AddMessage(ChatMessageResponse message)
		{
			if (_lines.Count >= MaxLines)
			{
				Destroy(_lines.Dequeue().Line.gameObject);
			}

			CanvasGroup line = Instantiate(linePrefab, messagesContainer);
			line.GetComponentInChildren<TextMeshProUGUI>().SetText(Format(message));
			_lines.Enqueue((line, Time.unscaledTime));
		}

		private static string Format(ChatMessageResponse message)
		{
			return message.Type switch
			{
				ChatMessageType.Player => $"<color={PlayerColor}>{Escape(message.Sender)}:</color> {Escape(message.Text)}",
				ChatMessageType.System => $"<color={SystemColor}>{Escape(message.Text)}</color>",
				_ => $"<color={ReplyColor}>{Escape(message.Text)}</color>"
			};
		}

		// Players' text is shown as is, without rich text tags.
		private static string Escape(string text)
		{
			return "<noparse>" + NoParseEnd.Replace(text ?? string.Empty, string.Empty) + "</noparse>";
		}
	}
}
