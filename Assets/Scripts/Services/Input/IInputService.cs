using UnityEngine;

namespace Services
{
	public interface IInputService
	{
		Vector2 Axis { get; }
		Vector2 MouseAxis { get; }
		Vector2 RawMouseAxis { get; }
		bool IsEnabled { get; }
		bool IsFirstActionButtonDown();
		bool IsFirstActionButtonUp();
		bool IsFirstActionButtonHold();
		bool IsSecondActionButtonDown();
		bool IsSecondActionButtonUp();
		bool IsSecondActionButtonHold();
		bool IsRotateButtonDown();
		bool IsBlueprintMenuButtonHold();
		bool IsReloadingButtonDown();
		bool IsJumpButtonDown();
		bool IsSprintButtonHold();
		float GetScrollSpeed();
		bool IsScoreboardButtonDown();
		bool IsScoreboardButtonUp();
		bool IsChooseClassButtonDown();
		bool IsInGameMenuButtonDown();
		bool IsMapButtonDown();
		bool IsMapButtonUp();
		bool IsPaletteButtonHold();
		bool IsLeftArrowButtonDown();
		bool IsRightArrowButtonDown();
		bool IsUpArrowButtonDown();
		bool IsDownArrowButtonDown();
		bool IsSlotButtonPressed(int number);
		void Enable();
		void Disable();
		bool IsScrollButtonDown();
	}
}
