using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PalettePointerMover
    {
        private const int PaletteSize = 8;

        public Action<int> PointerMoved;

        private readonly Transform _pointer;
        private readonly Image[] _blockImages;
        private int _indexX;
        private int _indexY;

        public PalettePointerMover(Transform pointer, Image[] blockImages)
        {
            _pointer = pointer;
            _blockImages = blockImages;
        }

        public void MoveLeft()
        {
            if (_indexX <= 0)
            {
                return;
            }

            _indexX--;
            MovePointer();
        }

        public void MoveRight()
        {
            if (_indexX >= PaletteSize - 1)
            {
                return;
            }

            _indexX += 1;
            MovePointer();
        }

        public void MoveUp()
        {
            if (_indexY <= 0)
            {
                return;
            }

            _indexY -= 1;
            MovePointer();
        }

        public void MoveDown()
        {
            if (_indexY >= PaletteSize - 1)
            {
                return;
            }

            _indexY += 1;
            MovePointer();
        }

        private void MovePointer()
        {
            _pointer.position = _blockImages[_indexX + _indexY * PaletteSize].transform.position;
            PointerMoved?.Invoke(_indexX + _indexY * PaletteSize);
        }
    }
}