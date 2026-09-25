using UnityEngine;

namespace Services
{
	public class StandaloneInputService : IInputService
	{
		private const int SlotCount = 10;

		private KeyBindingsData _bindings;
		private bool _isEnabled;

		public Vector2 Axis => _isEnabled
			? new Vector2(GetAxis(ControlAction.MoveBackward, ControlAction.MoveForward), GetAxis(ControlAction.MoveLeft, ControlAction.MoveRight))
			: Vector2.zero;

		public Vector2 MouseAxis => _isEnabled
			? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))
			: Vector2.zero;

		public Vector2 RawMouseAxis => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

		public bool IsEnabled => _isEnabled;

		public StandaloneInputService(IStorageService storageService)
		{
			_bindings = storageService.Load<KeyBindingsData>(IStorageService.KeyBindingsKey);
			storageService.Subscribe<KeyBindingsData>(bindings => _bindings = bindings);
		}

		public bool IsFirstActionButtonDown()
		{
			return IsDown(ControlAction.PrimaryAction) && _isEnabled;
		}

		public bool IsFirstActionButtonUp()
		{
			return IsUp(ControlAction.PrimaryAction) && _isEnabled;
		}

		public bool IsFirstActionButtonHold()
		{
			return IsHeld(ControlAction.PrimaryAction) && _isEnabled;
		}

		public bool IsSecondActionButtonDown()
		{
			return IsDown(ControlAction.SecondaryAction) && _isEnabled;
		}

		public bool IsSecondActionButtonUp()
		{
			return IsUp(ControlAction.SecondaryAction) && _isEnabled;
		}

		public bool IsSecondActionButtonHold()
		{
			return IsHeld(ControlAction.SecondaryAction) && _isEnabled;
		}

		public bool IsRotateButtonDown()
		{
			return IsDown(ControlAction.RotateBlueprint) && _isEnabled;
		}

		public bool IsBlueprintMenuButtonHold()
		{
			return IsHeld(ControlAction.BlueprintMenu);
		}

		public bool IsReloadingButtonDown()
		{
			return IsDown(ControlAction.Reload) && _isEnabled;
		}

		public bool IsJumpButtonDown()
		{
			return IsDown(ControlAction.Jump) && _isEnabled;
		}

		public bool IsSprintButtonHold()
		{
			return IsHeld(ControlAction.Sprint) && _isEnabled;
		}

		public float GetScrollSpeed()
		{
			return _isEnabled ? Input.GetAxis("Mouse ScrollWheel") : 0.0f;
		}

		public bool IsScrollButtonDown()
		{
			return _isEnabled && IsDown(ControlAction.PickColor);
		}

		public bool IsScoreboardButtonDown()
		{
			return IsDown(ControlAction.Scoreboard);
		}

		public bool IsScoreboardButtonUp()
		{
			return IsUp(ControlAction.Scoreboard);
		}

		public bool IsChooseClassButtonDown()
		{
			return IsDown(ControlAction.ChooseClass);
		}

		public bool IsInGameMenuButtonDown()
		{
			return Input.GetKeyDown(KeyCode.Escape);
		}

		public bool IsMapButtonDown()
		{
			return IsDown(ControlAction.Map);
		}

		public bool IsMapButtonUp()
		{
			return IsUp(ControlAction.Map);
		}

		public bool IsChatButtonDown()
		{
			return IsDown(ControlAction.Chat);
		}

		public bool IsPaletteButtonHold()
		{
			return IsHeld(ControlAction.Palette) && _isEnabled;
		}

		public bool IsLeftArrowButtonDown()
		{
			return IsDown(ControlAction.PaletteLeft) && _isEnabled;
		}

		public bool IsRightArrowButtonDown()
		{
			return IsDown(ControlAction.PaletteRight) && _isEnabled;
		}

		public bool IsUpArrowButtonDown()
		{
			return IsDown(ControlAction.PaletteUp) && _isEnabled;
		}

		public bool IsDownArrowButtonDown()
		{
			return IsDown(ControlAction.PaletteDown) && _isEnabled;
		}

		public bool IsSlotButtonPressed(int number)
		{
			if (!_isEnabled || number < 0 || number >= SlotCount)
			{
				return false;
			}

			return IsDown(ControlAction.Slot1 + number);
		}

		public void Enable() // TODO: This method probably does not belong here
		{
			_isEnabled = true;
		}

		public void Disable()
		{
			_isEnabled = false;
		}

		private bool IsDown(ControlAction action)
		{
			return Input.GetKeyDown(_bindings.GetKey(action));
		}

		private bool IsUp(ControlAction action)
		{
			return Input.GetKeyUp(_bindings.GetKey(action));
		}

		private bool IsHeld(ControlAction action)
		{
			return Input.GetKey(_bindings.GetKey(action));
		}

		private float GetAxis(ControlAction negative, ControlAction positive)
		{
			return (IsHeld(positive) ? 1.0f : 0.0f) - (IsHeld(negative) ? 1.0f : 0.0f);
		}
	}
}
