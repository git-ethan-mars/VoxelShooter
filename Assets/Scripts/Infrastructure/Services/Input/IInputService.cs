﻿using UnityEngine;

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
        bool IsScoreboardButtonDown();
        bool IsScoreboardButtonUp();
        bool IsChooseClassButtonDown();
        bool IsInGameMenuButtonDown();
        bool IsLeftArrowButtonDown();
        bool IsRightArrowButtonDown();
        bool IsUpArrowButtonDown();
        bool IsDownArrowButtonDown();
        bool IsZActionButtonDown();
        bool IsXActionButtonDown();
        bool IsCActionButtonDown();
        void Enable();
        void Disable();
    }
}