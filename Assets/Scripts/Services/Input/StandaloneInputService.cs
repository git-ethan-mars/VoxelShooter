using UnityEngine;
namespace Services
{
	public class StandaloneInputService : IInputService
	{
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

		public bool IsFirstSlotButtonPressed()
		{
			return Input.GetKeyDown(KeyCode.Alpha1) && _isEnabled;
		}

		public bool IsSecondSlotButtonPressed()
		{
			return Input.GetKeyDown(KeyCode.Alpha2) && _isEnabled;
		}

		public bool IsThirdSlotButtonPressed()
		{
			return Input.GetKeyDown(KeyCode.Alpha3) && _isEnabled;
		}

		public bool IsFourthSlotButtonPressed()
		{
			return Input.GetKeyDown(KeyCode.Alpha4) && _isEnabled;
		}

		public bool IsFifthSlotButtonPressed()
		{
			return Input.GetKeyDown(KeyCode.Alpha5) && _isEnabled;
		}

		public void Enable() // TODO : Кажется этот метод не должен тут быть
		{
			_isEnabled = true;
		}

		public void Disable()
		{
			_isEnabled = false;
		}
	}
}