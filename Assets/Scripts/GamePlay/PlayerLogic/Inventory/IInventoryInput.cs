using System;

namespace Inventory
{
    public interface IInventoryInput
    {
        event Action MouseScrolledUp;
        event Action MouseScrolledDown;
        event Action FirstActionButtonDown;
        event Action FirstActionButtonUp;
        event Action FirstActionButtonHold;
        event Action SecondActionButtonDown;
        event Action SecondActionButtonUp;
        event Action<int> SlotButtonPressed;
        event Action ReloadButtonDown;
    }
}