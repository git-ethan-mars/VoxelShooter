using UnityEngine;

namespace Infrastructure.Services.Input
{
    public interface IInputService : IService
    {
        Vector2 Axis { get; }
        bool IsFirstActionButtonDown();
        bool IsSecondActionButtonDown();
        bool IsReloadingButtonDown();
        bool IsFirstActionButtonHold();
        bool IsSecondActionButtonHold();
        bool IsJumpButtonDown();
    }
}