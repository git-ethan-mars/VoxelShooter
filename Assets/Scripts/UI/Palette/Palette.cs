using System.Collections;
using System.Linq;
using Infrastructure;
using Infrastructure.Services.Input;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Palette
{
    public class Palette : MonoBehaviour
    {
        public ObservableVariable<Color32> Color;

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

        private IEnumerator coroutine;

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

            Color = new ObservableVariable<Color32>(_colorCreator.ColorById[0]);
        }

        private void OnEnable()
        {
            coroutine = ChangePointerColor();
            StartCoroutine(coroutine);
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
            
            if (_inputService.IsSecondActionButtonDown())
            {
                if (_pointerMover.GetXPosition() == _pointerMover.PaletteSize - 1)
                {
                    _pointerMover.SetXPosition(0);
                    _pointerMover.SetYPosition(_pointerMover.GetYPosition() + 1);
                    if (_pointerMover.GetYPosition() == _pointerMover.PaletteSize)
                    {
                        _pointerMover.SetYPosition(0);
                    }
                    _pointerMover.MovePointer();
                }
                else
                {
                    _pointerMover.MoveRight();
                }
            }
        }

        private void OnDisable()
        {
            StopCoroutine(coroutine);
        }

        private void OnDestroy()
        {
            _pointerMover.PointerMoved -= OnPointerMoved;
        }

        private void OnPointerMoved(int index)
        {
            Color.Value = _colorCreator.ColorById[index];
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