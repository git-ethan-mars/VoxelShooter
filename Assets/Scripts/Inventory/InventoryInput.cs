﻿using System;
using System.Collections.Generic;
using Infrastructure.Services.Input;
using Mirror;
using UnityEngine;

namespace Inventory
{
    public class InventoryInput : NetworkBehaviour
    {
        public event Action OnScroll;
        public event Action OnFirstActionButtonDown;
        public event Action OnFirstActionButtonHold;
        public event Action OnSecondActionButtonDown;
        public event Action<int> OnChangeSlot;

        private IInputService _inputService;


        private readonly Dictionary<KeyCode, int> _slotIndexByKey = new()
        {
            [KeyCode.Alpha1] = 0,
            [KeyCode.Alpha2] = 1,
            [KeyCode.Alpha3] = 2,
            [KeyCode.Alpha4] = 3,
            [KeyCode.Alpha5] = 4,
        };

        

        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            if (_inputService.GetScrollSpeed() != 0.0f)
            {
                OnScroll?.Invoke();
            }

            foreach (var (key, index) in _slotIndexByKey)
            {
                if (Input.GetKeyDown(key))
                {
                    OnChangeSlot?.Invoke(index);
                }
            }

            if (_inputService.IsFirstActionButtonDown())
            {
                OnFirstActionButtonDown?.Invoke();
            }

            if (_inputService.IsSecondActionButtonDown())
            {
                OnSecondActionButtonDown?.Invoke();
            }

            if (_inputService.IsFirstActionButtonHold())
            {
                OnFirstActionButtonHold?.Invoke();
            }
        }
    }
}