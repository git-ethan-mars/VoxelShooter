using System;
using System.Collections;
using System.Linq;
using Infrastructure.Services.Input;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Palette : MonoBehaviour
    {
        public Action<Color32> ColorChanged;

        private const float SwitchColorTime = 0.25f;

        [SerializeField]
        private Image[] blockImages;

        [SerializeField]
        private Image pointer;

        [SerializeField]
        private Sprite blackBoarder;

        [SerializeField]
        private Sprite blueBoarder;

        private IInputService _inputService;
        private PalettePointerMover _pointerMover;
        private PaletteColorCreator _colorCreator;

        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
            _colorCreator = new PaletteColorCreator();
            _pointerMover = new PalettePointerMover(pointer.transform, blockImages);
            _pointerMover.PointerMoved += OnPointerMoved;
            var colors = _colorCreator.ColorById.Values.ToList();
            for (var i = 0; i < colors.Count; i++)
            {
                blockImages[i].color = colors[i];
            }

            ColorChanged?.Invoke(_colorCreator.ColorById[0]);
        }

        private void OnEnable()
        {
            StartCoroutine(ChangePointerColor());
        }

        private void Update()
        {
            if (_inputService.IsLeftArrowButtonDown())
            {
                _pointerMover.MoveLeft();
            }

            if (_inputService.IsRightArrowButtonDown())
            {
                _pointerMover.MoveRight();
            }

            if (_inputService.IsUpArrowButtonDown())
            {
                _pointerMover.MoveUp();
            }

            if (_inputService.IsDownArrowButtonDown())
            {
                _pointerMover.MoveDown();
            }
        }

        private void OnDisable()
        {
            StopCoroutine(ChangePointerColor());
        }

        private void OnDestroy()
        {
            _pointerMover.PointerMoved -= OnPointerMoved;
        }

        private void OnPointerMoved(int index)
        {
            ColorChanged?.Invoke(_colorCreator.ColorById[index]);
        }

        private IEnumerator ChangePointerColor()
        {
            while (true)
            {
                pointer.sprite = blackBoarder;
                yield return new WaitForSeconds(SwitchColorTime);
                pointer.sprite = blueBoarder;
                yield return new WaitForSeconds(SwitchColorTime);
            }
        }
    }
}