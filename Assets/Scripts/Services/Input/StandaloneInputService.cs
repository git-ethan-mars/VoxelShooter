using UnityEngine;

namespace Services
{
	public class StandaloneInputService : IInputService
	{
		private static readonly KeyCode[] BlueprintKeys = { KeyCode.Z, KeyCode.X, KeyCode.C, KeyCode.V };

		private bool _isEnabled;

		public Vector2 Axis => _isEnabled
			? new Vector2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"))
			: Vector2.zero;

		public Vector2 MouseAxis => _isEnabled
			? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"))
			: Vector2.zero;

		public bool IsFirstActionButtonDown()
		{
			return Input.GetMouseButtonDown(0) && _isEnabled;
		}

		public bool IsFirstActionButtonUp()
		{
			return Input.GetMouseButtonUp(0) && _isEnabled;
		}

		public bool IsFirstActionButtonHold()
		{
			return Input.GetMouseButton(0) && _isEnabled;
		}

		public bool IsSecondActionButtonDown()
		{
			return Input.GetMouseButtonDown(1) && _isEnabled;
		}

		public bool IsSecondActionButtonUp()
		{
			return Input.GetMouseButtonUp(1) && _isEnabled;
		}

		public bool IsReloadingButtonDown()
		{
			return Input.GetKeyDown(KeyCode.R) && _isEnabled;
		}

		public bool IsJumpButtonDown()
		{
			return Input.GetKeyDown(KeyCode.Space) && _isEnabled;
		}

		public bool IsSprintButtonHold()
		{
			return Input.GetKey(KeyCode.LeftShift) && _isEnabled;
		}

		public float GetScrollSpeed()
		{
			return _isEnabled ? Input.GetAxis("Mouse ScrollWheel") : 0.0f;
		}

		public bool IsScrollButtonDown()
		{
			return _isEnabled && Input.GetMouseButtonDown(2);
		}

		public bool IsScoreboardButtonDown()
		{
			return Input.GetKeyDown(KeyCode.Tab);
		}

		public bool IsScoreboardButtonUp()
		{
			return Input.GetKeyUp(KeyCode.Tab);
		}

		public bool IsChooseClassButtonDown()
		{
			return Input.GetKeyDown(KeyCode.N);
		}

		public bool IsInGameMenuButtonDown()
		{
			return Input.GetKeyDown(KeyCode.Escape);
		}

		public bool IsMapButtonDown()
		{
			return Input.GetKeyDown(KeyCode.M);
		}

		public bool IsMapButtonUp()
		{
			return Input.GetKeyUp(KeyCode.M);
		}

		public bool IsLeftArrowButtonDown()
		{
			return Input.GetKeyDown(KeyCode.LeftArrow) && _isEnabled;
		}

		public bool IsRightArrowButtonDown()
		{
			return Input.GetKeyDown(KeyCode.RightArrow) && _isEnabled;
		}

		public bool IsUpArrowButtonDown()
		{
			return Input.GetKeyDown(KeyCode.UpArrow) && _isEnabled;
		}

		public bool IsDownArrowButtonDown()
		{
			return Input.GetKeyDown(KeyCode.DownArrow) && _isEnabled;
		}

		public bool IsSlotButtonPressed(int number)
		{
			if (!_isEnabled)
			{
				return false;
			}

			if (number == 0)
			{
				return Input.GetKeyDown(KeyCode.Alpha1);
			}

			if (number == 1)
			{
				return Input.GetKeyDown(KeyCode.Alpha2);
			}

			if (number == 2)
			{
				return Input.GetKeyDown(KeyCode.Alpha3);
			}

			if (number == 3)
			{
				return Input.GetKeyDown(KeyCode.Alpha4);
			}

			if (number == 4)
			{
				return Input.GetKeyDown(KeyCode.Alpha5);
			}

			if (number == 5)
			{
				return Input.GetKeyDown(KeyCode.Alpha6);
			}

			if (number == 6)
			{
				return Input.GetKeyDown(KeyCode.Alpha7);
			}

			if (number == 7)
			{
				return Input.GetKeyDown(KeyCode.Alpha8);
			}

			if (number == 8)
			{
				return Input.GetKeyDown(KeyCode.Alpha9);
			}

			if (number == 9)
			{
				return Input.GetKeyDown(KeyCode.Alpha0);
			}

			return false;
		}

		public bool IsBlueprintButtonDown(int number)
		{
			if (!_isEnabled || number < 0 || number >= BlueprintKeys.Length)
			{
				return false;
			}

			return Input.GetKeyDown(BlueprintKeys[number]);
		}

		public void Enable() // TODO: This method probably does not belong here
		{
			_isEnabled = true;
		}

		public void Disable()
		{
			_isEnabled = false;
		}
	}
}
