using System.Collections.Generic;
using Data;
using GamePlay;
using Reflex.Attributes;
using Services;
using TMPro;
using UnityEngine;

namespace UI
{
	public class BlueprintMenuView : MonoBehaviour
	{
		private const float RadiusX = 260.0f;
		private const float RadiusY = 175.0f;
		private const float PointerSensitivity = 0.08f;
		private const float SelectionDeadZone = 0.35f;
		private const float PointerIndicatorRadius = 70.0f;

		[SerializeField] private CanvasGroup canvasGroup;
		[SerializeField] private RectTransform entriesContainer;
		[SerializeField] private BlueprintMenuEntry entryPrefab;
		[SerializeField] private RectTransform pointerIndicator;
		[SerializeField] private TextMeshProUGUI selectedLayoutName;

		private readonly List<BlueprintMenuEntry> _entries = new List<BlueprintMenuEntry>();

		private IInputService _inputService;
		private Blueprint _blueprint;
		private Vector2 _pointer;
		private int _highlightedIndex;
		private bool _isOpen;

		[Inject]
		private void Construct(IInputService inputService)
		{
			_inputService = inputService;
		}

		private void Awake()
		{
			SetVisible(false);
		}

		private void Update()
		{
			if (_blueprint == null)
			{
				return;
			}

			bool isMenuButtonHeld = _inputService.IsBlueprintMenuButtonHold();

			if (!_isOpen && isMenuButtonHeld && _inputService.IsEnabled)
			{
				Open();
			}
			else if (_isOpen && !isMenuButtonHeld)
			{
				Close(true);
			}

			if (_isOpen)
			{
				UpdatePointer();
			}
		}

		public void Bind(Blueprint blueprint)
		{
			_blueprint = blueprint;
		}

		public void Unbind()
		{
			if (_isOpen)
			{
				Close(false);
			}

			_blueprint = null;
		}

		private void Open()
		{
			IReadOnlyList<BlueprintLayout> layouts = _blueprint.Layouts;
			ClearEntries();

			for (int i = 0; i < layouts.Count; i++)
			{
				BlueprintMenuEntry entry = Instantiate(entryPrefab, entriesContainer);
				float angle = i * Mathf.PI * 2.0f / layouts.Count;
				((RectTransform)entry.transform).anchoredPosition = new Vector2(Mathf.Sin(angle) * RadiusX, Mathf.Cos(angle) * RadiusY);
				entry.Initialize(layouts[i]);
				_entries.Add(entry);
			}

			_pointer = Vector2.zero;
			Highlight(_blueprint.LayoutIndex);
			_isOpen = true;
			_inputService.Disable();
			SetVisible(true);
		}

		private void Close(bool applySelection)
		{
			if (applySelection && _highlightedIndex != _blueprint.LayoutIndex)
			{
				_blueprint.SelectLayout(_highlightedIndex);
			}

			_isOpen = false;
			_inputService.Enable();
			SetVisible(false);
			ClearEntries();
		}

		private void UpdatePointer()
		{
			_pointer = Vector2.ClampMagnitude(_pointer + _inputService.RawMouseAxis * PointerSensitivity, 1.0f);
			pointerIndicator.anchoredPosition = _pointer * PointerIndicatorRadius;

			if (_pointer.magnitude < SelectionDeadZone || _entries.Count == 0)
			{
				return;
			}

			float angle = Mathf.Atan2(_pointer.x, _pointer.y) * Mathf.Rad2Deg;

			if (angle < 0.0f)
			{
				angle += 360.0f;
			}

			float sectorSize = 360.0f / _entries.Count;
			Highlight(Mathf.RoundToInt(angle / sectorSize) % _entries.Count);
		}

		private void Highlight(int index)
		{
			_highlightedIndex = index;

			for (int i = 0; i < _entries.Count; i++)
			{
				_entries[i].SetHighlighted(i == index);
			}

			BlueprintLayout layout = _blueprint.Layouts[index];
			selectedLayoutName.SetText($"{layout.Name}\n<size=70%>{layout.Positions.Count} blocks</size>");
		}

		private void ClearEntries()
		{
			foreach (BlueprintMenuEntry entry in _entries)
			{
				Destroy(entry.gameObject);
			}

			_entries.Clear();
		}

		private void SetVisible(bool isVisible)
		{
			canvasGroup.alpha = isVisible ? 1.0f : 0.0f;
		}
	}
}
