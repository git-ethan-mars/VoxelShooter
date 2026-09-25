using System;
using System.Collections.Generic;
using System.Linq;
using R3;
using Reflex.Attributes;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class KeyBindingsView : MonoBehaviour
	{
		private const string ListeningText = "PRESS A KEY";

		// Groups and labels of the rows, in display order. A null action starts a new group.
		private static readonly (ControlAction? Action, string Label)[] Rows =
		{
			(null, "MOVEMENT"),
			(ControlAction.MoveForward, "FORWARD"),
			(ControlAction.MoveBackward, "BACKWARD"),
			(ControlAction.MoveLeft, "LEFT"),
			(ControlAction.MoveRight, "RIGHT"),
			(ControlAction.Jump, "JUMP"),
			(ControlAction.Sprint, "SPRINT"),
			(null, "ACTIONS"),
			(ControlAction.PrimaryAction, "SHOOT / BUILD"),
			(ControlAction.SecondaryAction, "AIM / BLOCK LINE"),
			(ControlAction.Reload, "RELOAD"),
			(ControlAction.RotateBlueprint, "ROTATE BLUEPRINT"),
			(ControlAction.BlueprintMenu, "BLUEPRINT MENU"),
			(ControlAction.PickColor, "PICK COLOR"),
			(null, "PALETTE"),
			(ControlAction.Palette, "OPEN PALETTE"),
			(ControlAction.PaletteUp, "PALETTE UP"),
			(ControlAction.PaletteDown, "PALETTE DOWN"),
			(ControlAction.PaletteLeft, "PALETTE LEFT"),
			(ControlAction.PaletteRight, "PALETTE RIGHT"),
			(null, "INTERFACE"),
			(ControlAction.Scoreboard, "SCOREBOARD"),
			(ControlAction.ChooseClass, "CHOOSE CLASS"),
			(ControlAction.Map, "MAP"),
			(null, "INVENTORY"),
			(ControlAction.Slot1, "SLOT 1"),
			(ControlAction.Slot2, "SLOT 2"),
			(ControlAction.Slot3, "SLOT 3"),
			(ControlAction.Slot4, "SLOT 4"),
			(ControlAction.Slot5, "SLOT 5"),
			(ControlAction.Slot6, "SLOT 6"),
			(ControlAction.Slot7, "SLOT 7"),
			(ControlAction.Slot8, "SLOT 8"),
			(ControlAction.Slot9, "SLOT 9"),
			(ControlAction.Slot10, "SLOT 10"),
		};

		// Escape cancels listening, so it can not be bound; joystick codes are not supported.
		private static readonly KeyCode[] BindableKeys = ((KeyCode[])Enum.GetValues(typeof(KeyCode)))
			.Where(key => key != KeyCode.None && key != KeyCode.Escape && key < KeyCode.JoystickButton0)
			.Distinct()
			.ToArray();

		[SerializeField] private KeyBindingRowView rowPrefab;
		[SerializeField] private RectTransform content;
		[SerializeField] private Button resetButton;

		private readonly Dictionary<ControlAction, KeyBindingRowView> _rowsByAction = new Dictionary<ControlAction, KeyBindingRowView>();

		private IStorageService _storageService;
		private ControlAction? _listeningAction;
		private bool _isBuilt;

		[Inject]
		private void Construct(IStorageService storageService)
		{
			_storageService = storageService;
		}

		private void Update()
		{
			if (_listeningAction == null)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.Escape))
			{
				StopListening();
				return;
			}

			foreach (KeyCode key in BindableKeys)
			{
				if (!Input.GetKeyDown(key))
				{
					continue;
				}

				KeyBindingsData bindings = _storageService.Load<KeyBindingsData>(IStorageService.KeyBindingsKey);
				_storageService.Set(bindings.WithBinding(_listeningAction.Value, key));
				StopListening();
				Refresh();
				return;
			}
		}

		private void OnDisable()
		{
			StopListening();
		}

		public void Show()
		{
			Build();
			Refresh();
		}

		private void Build()
		{
			if (_isBuilt)
			{
				return;
			}

			_isBuilt = true;

			foreach ((ControlAction? action, string rowLabel) in Rows)
			{
				KeyBindingRowView row = Instantiate(rowPrefab, content);

				if (action == null)
				{
					row.InitializeHeader(rowLabel);
					continue;
				}

				ControlAction rowAction = action.Value;
				row.InitializeAction(rowLabel);
				row.Clicked.Subscribe(_ => StartListening(rowAction)).AddTo(this);
				_rowsByAction[rowAction] = row;
			}

			resetButton.OnClickAsObservable()
				.Subscribe(_ => ResetToDefaults())
				.AddTo(this);
		}

		private void Refresh()
		{
			KeyBindingsData bindings = _storageService.Load<KeyBindingsData>(IStorageService.KeyBindingsKey);

			foreach ((ControlAction action, KeyBindingRowView row) in _rowsByAction)
			{
				bool isListening = action == _listeningAction;
				row.SetListening(isListening);
				row.SetKey(isListening ? ListeningText : GetKeyName(bindings.GetKey(action)));
			}
		}

		private void StartListening(ControlAction action)
		{
			_listeningAction = action;
			Refresh();
		}

		private void StopListening()
		{
			if (_listeningAction == null)
			{
				return;
			}

			_listeningAction = null;
			Refresh();
		}

		private void ResetToDefaults()
		{
			_listeningAction = null;
			_storageService.Set(new KeyBindingsData());
			Refresh();
		}

		private static string GetKeyName(KeyCode key)
		{
			return key switch
			{
				KeyCode.Mouse0 => "LMB",
				KeyCode.Mouse1 => "RMB",
				KeyCode.Mouse2 => "MMB",
				>= KeyCode.Mouse3 and <= KeyCode.Mouse6 => "MOUSE " + (key - KeyCode.Mouse0 + 1),
				>= KeyCode.Alpha0 and <= KeyCode.Alpha9 => ((int)(key - KeyCode.Alpha0)).ToString(),
				>= KeyCode.Keypad0 and <= KeyCode.Keypad9 => "NUM " + (key - KeyCode.Keypad0),
				KeyCode.LeftShift => "L SHIFT",
				KeyCode.RightShift => "R SHIFT",
				KeyCode.LeftControl => "L CTRL",
				KeyCode.RightControl => "R CTRL",
				KeyCode.LeftAlt => "L ALT",
				KeyCode.RightAlt => "R ALT",
				KeyCode.UpArrow => "UP ARROW",
				KeyCode.DownArrow => "DOWN ARROW",
				KeyCode.LeftArrow => "LEFT ARROW",
				KeyCode.RightArrow => "RIGHT ARROW",
				KeyCode.Return => "ENTER",
				KeyCode.BackQuote => "`",
				_ => key.ToString().ToUpperInvariant(),
			};
		}
	}
}
