using UnityEngine;

namespace Services
{
	public interface IInputService
	{
		Vector2 Axis { get; }
		Vector2 MouseAxis { get; }
		bool IsFirstActionButtonDown();
		bool IsFirstActionButtonUp();
		bool IsFirstActionButtonHold();
		bool IsSecondActionButtonDown();
		bool IsSecondActionButtonUp();
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
		bool IsLeftArrowButtonDown();
		bool IsRightArrowButtonDown();
		bool IsUpArrowButtonDown();
		bool IsDownArrowButtonDown();
		bool IsSlotButtonPressed(int number);
		bool IsBlueprintButtonDown(int number);
		void Enable();
		void Disable();
		bool IsScrollButtonDown();
	}
}
