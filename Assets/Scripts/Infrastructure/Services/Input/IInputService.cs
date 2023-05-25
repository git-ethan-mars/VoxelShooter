using UnityEngine;

namespace Infrastructure.Services.Input
{
    public interface IInputService : IService
    {
        Vector2 Axis { get; }
        bool IsFirstActionButtonDown();
        bool IsFirstActionButtonUp();
        bool IsFirstActionButtonHold();
        bool IsSecondActionButtonDown();
        bool IsSecondActionButtonUp();
        bool IsReloadingButtonDown();
        bool IsJumpButtonDown();
        float GetScrollSpeed();
        float GetMouseVerticalAxis();
        float GetMouseHorizontalAxis();
        bool IsScoreboardButtonHold();
        bool IsChooseClassButtonDown();
    }
}