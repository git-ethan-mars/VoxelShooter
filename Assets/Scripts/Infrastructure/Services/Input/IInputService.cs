using UnityEngine;

namespace Infrastructure.Services.Input
{
    public interface IInputService : IService
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
        float GetScrollSpeed();
        bool IsScoreboardButtonHold();
        bool IsChooseClassButtonDown();
        bool IsInGameMenuButtonDown();
        bool IsLeftArrowButtonDown();
        bool IsRightArrowButtonDown();
        bool IsUpArrowButtonDown();
        bool IsDownArrowButtonDown();
    }
}