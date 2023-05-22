using UnityEngine;

namespace Infrastructure.Services.Input
{
    public interface IInputService : IService
    {
        Vector2 Axis { get; }
        bool IsFirstActionButtonDown();
        bool IsFirstActionButtonUp();
        bool IsSecondActionButtonDown();
        bool IsReloadingButtonDown();
        bool IsFirstActionButtonHold();
        bool IsJumpButtonDown();
        float GetScrollSpeed();
        float GetMouseVerticalAxis();
        float GetMouseHorizontalAxis();
        bool IsScoreboardButtonHold();
        bool IsChooseClassButtonDown();
    }
}