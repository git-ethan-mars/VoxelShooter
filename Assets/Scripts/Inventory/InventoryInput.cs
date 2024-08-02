using System;
using System.Collections.Generic;
using Infrastructure.Services.Input;
using UnityEngine;

namespace Inventory
{
    public class InventoryInput : IInventoryInput
    {
        public event Action ZActionButtonDown;
        public event Action XActionButtonDown;
        public event Action CActionButtonDown;
        public event Action MouseScrolledUp;
        public event Action MouseScrolledDown;
        public event Action FirstActionButtonDown;
        public event Action FirstActionButtonUp;
        public event Action FirstActionButtonHold;
        public event Action SecondActionButtonDown;
        public event Action SecondActionButtonUp;
        public event Action ReloadButtonDown;
        public event Action<int> SlotButtonPressed;

        private readonly IInputService _inputService;


        private readonly Dictionary<KeyCode, int> _slotIndexByKey = new()
        {
            [KeyCode.Alpha1] = 0,
            [KeyCode.Alpha2] = 1,
            [KeyCode.Alpha3] = 2,
            [KeyCode.Alpha4] = 3,
            [KeyCode.Alpha5] = 4,
        };

        public InventoryInput(IInputService inputService)
        {
            _inputService = inputService;
        }

        public void Update()
        {
            var mouseScrollSpeed = _inputService.GetScrollSpeed();
            if (mouseScrollSpeed > 0)
            {
                MouseScrolledUp?.Invoke();
            }

            if (mouseScrollSpeed < 0)
            {
                MouseScrolledDown?.Invoke();
            }

            foreach (var (key, index) in _slotIndexByKey)
            {
                if (Input.GetKeyDown(key))
                {
                    SlotButtonPressed?.Invoke(index);
                }
            }

            if (_inputService.IsFirstActionButtonDown())
            {
                FirstActionButtonDown?.Invoke();
            }

            if (_inputService.IsFirstActionButtonHold())
            {
                FirstActionButtonHold?.Invoke();
            }

            if (_inputService.IsFirstActionButtonUp())
            {
                FirstActionButtonUp?.Invoke();
            }

            if (_inputService.IsSecondActionButtonDown())
            {
                SecondActionButtonDown?.Invoke();
            }

            if (_inputService.IsSecondActionButtonUp())
            {
                SecondActionButtonUp?.Invoke();
            }

            if (_inputService.IsReloadingButtonDown())
            {
                ReloadButtonDown?.Invoke();
            }

            if (_inputService.IsZActionButtonDown())
            {
                ZActionButtonDown?.Invoke();
            }
            
            if (_inputService.IsXActionButtonDown())
            {
                XActionButtonDown?.Invoke();
            }

            if (_inputService.IsCActionButtonDown())
            {
                CActionButtonDown?.Invoke();
            }
        }
    }
}