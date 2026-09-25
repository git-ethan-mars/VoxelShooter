using System;
using GamePlay;
using R3;
using Reflex.Attributes;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelMap;

namespace UI
{
	public class PaletteView : MonoBehaviour
	{
		private const string CollapsedHint = "HOLD Q";
		private const string ExpandedHint = "ARROWS";

		[SerializeField] private Image currentColor;
		[SerializeField] private PaletteElementListView recentColors;
		[SerializeField] private GameObject expandedSection;
		[SerializeField] private GameObject mapColorsSection;
		[SerializeField] private PaletteElementListView mapColors;
		[SerializeField] private PaletteElementListView paletteColors;
		[SerializeField] private GridLayoutGroup paletteGrid;
		[SerializeField] private TextMeshProUGUI hint;

		private IInputService _inputService;
		private CharacterProvider _characterProvider;
		private RectPalette _palette;
		private IDisposable _characterSubscription;
		private bool _isBuilt;
		private bool _isExpanded;

		[Inject]
		private void Construct(IInputService inputService, IStaticDataService staticData, CharacterProvider characterProvider,
			MapProvider mapProvider)
		{
			_inputService = inputService;
			_characterProvider = characterProvider;
			_palette = new RectPalette(staticData.GetRectPaletteData());

			mapProvider.Map
				.Where(map => map != null)
				.Subscribe(map => _palette.SetMapColors(MapColorSampler.GetDominantColors(map.MapData, _palette.ColumnCount)))
				.AddTo(this);
		}

		private void Update()
		{
			bool isExpanded = _inputService.IsPaletteButtonHold();

			if (isExpanded != _isExpanded)
			{
				SetExpanded(isExpanded);
			}

			if (_inputService.IsUpArrowButtonDown())
			{
				_palette.MovePointer(0, -1, _isExpanded);
			}

			if (_inputService.IsDownArrowButtonDown())
			{
				_palette.MovePointer(0, 1, _isExpanded);
			}

			if (_inputService.IsRightArrowButtonDown())
			{
				_palette.MovePointer(1, 0, _isExpanded);
			}

			if (_inputService.IsLeftArrowButtonDown())
			{
				_palette.MovePointer(-1, 0, _isExpanded);
			}
		}

		private void OnDestroy()
		{
			_characterSubscription?.Dispose();
		}

		public void Show()
		{
			Build();
			gameObject.SetActive(true);
			SetExpanded(false);

			_characterSubscription?.Dispose();
			Character character = _characterProvider.Character.Value;

			if (character == null)
			{
				return;
			}

			ReactiveProperty<Color32> desiredVoxelColor = character.Inventory.DesiredVoxelColor;
			_characterSubscription = Disposable.Combine(
				_palette.SelectedColor.Subscribe(color => desiredVoxelColor.Value = color),
				desiredVoxelColor.Subscribe(OnDesiredVoxelColorChanged));
		}

		public void Hide()
		{
			_characterSubscription?.Dispose();
			_characterSubscription = null;

			if (_isExpanded)
			{
				SetExpanded(false);
			}

			gameObject.SetActive(false);
		}

		private void Build()
		{
			if (_isBuilt)
			{
				return;
			}

			_isBuilt = true;

			paletteGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			paletteGrid.constraintCount = _palette.ColumnCount;
			paletteColors.SetColors(_palette.Colors);
			OnRecentColorsChanged();
			OnMapColorsChanged();

			_palette.SelectedColor.Subscribe(OnSelectedColorChanged).AddTo(this);
			_palette.RecentColorsChanged.Subscribe(_ => OnRecentColorsChanged()).AddTo(this);
			_palette.MapColorsChanged.Subscribe(_ => OnMapColorsChanged()).AddTo(this);
		}

		private void SetExpanded(bool isExpanded)
		{
			if (_isExpanded && !isExpanded)
			{
				_palette.CommitSelectedColor();
			}

			if (isExpanded)
			{
				_palette.FocusSelectedColor();
			}

			_isExpanded = isExpanded;
			expandedSection.SetActive(isExpanded);
			hint.SetText(isExpanded ? ExpandedHint : CollapsedHint);
			HighlightSelectedColor();
		}

		private void OnDesiredVoxelColorChanged(Color32 color)
		{
			// Transparent means the character has no color yet; any other change comes from the color picker.
			if (color.a == 0 || RectPalette.AreSame(color, _palette.SelectedColor.CurrentValue))
			{
				return;
			}

			_palette.PickColor(color);
		}

		private void OnSelectedColorChanged(Color32 color)
		{
			currentColor.color = color;
			HighlightSelectedColor();
		}

		private void OnRecentColorsChanged()
		{
			recentColors.SetColors(_palette.RecentColors);
			HighlightSelectedColor();
		}

		private void OnMapColorsChanged()
		{
			mapColors.SetColors(_palette.MapColors);
			mapColorsSection.SetActive(_palette.MapColors.Count > 0);
			HighlightSelectedColor();
		}

		private void HighlightSelectedColor()
		{
			Color32 color = _palette.SelectedColor.CurrentValue;
			recentColors.Highlight(color);
			mapColors.Highlight(color);
			paletteColors.Highlight(color);
		}
	}
}
